// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Sitecore.Rocks.Shell.Environment
{
    public class MockHost
    {
        private readonly Dictionary<int, IsolatedMethod> _isolatedMethods = new Dictionary<int, IsolatedMethod>();

        private readonly Dictionary<int, List<IsolatedSpecificMethod>> _isolatedSpecificMethods = new Dictionary<int, List<IsolatedSpecificMethod>>();

        public bool IsActive { get; set; }

        public bool Get(out object value, params object[] argList)
        {
            value = null;

            if (!IsActive)
            {
                return false;
            }

            var frame = new StackFrame(1);
            var method = (MethodInfo)frame.GetMethod();
            var hashCode = method.GetHashCode();

            var declaringType = method.DeclaringType;
            if (declaringType == null)
            {
                return false;
            }

            if (method.IsVirtual && declaringType.Name.StartsWith("Mocked"))
            {
                hashCode = method.GetBaseDefinition().GetHashCode();
            }

            List<IsolatedSpecificMethod> list;
            if (_isolatedSpecificMethods.TryGetValue(hashCode, out list))
            {
                foreach (var isolatedSpecificMethod in list)
                {
                    if (isolatedSpecificMethod.Arguments.Length != argList.Length)
                    {
                        continue;
                    }

                    if (!isolatedSpecificMethod.Arguments.Where((t, index) => t != argList[index]).Any())
                    {
                        value = isolatedSpecificMethod.ReturnValue();
                        return true;
                    }
                }
            }

            IsolatedMethod method2;
            if (_isolatedMethods.TryGetValue(hashCode, out method2))
            {
                value = method2.ReturnValue();
                return true;
            }

            return false;
        }

        public void Isolate(Expression<Action> action, Func<object> returnValue)
        {
            var member = action.Body as MethodCallExpression;
            if (member == null)
            {
                throw new Exception("Not a method call");
            }

            var hashCode = member.Method.GetHashCode();

            IsolatedMethod method;
            if (_isolatedMethods.TryGetValue(hashCode, out method))
            {
                throw new Exception("Method already isolated");
            }

            _isolatedMethods[hashCode] = new IsolatedMethod(returnValue);
        }

        public void IsolateSpecific(Expression<Action> action, Func<object> returnValue)
        {
            var member = action.Body as MethodCallExpression;
            if (member == null)
            {
                throw new Exception("Not a method call");
            }

            var hashCode = member.Method.GetHashCode();

            List<IsolatedSpecificMethod> list;
            if (!_isolatedSpecificMethods.TryGetValue(hashCode, out list))
            {
                list = new List<IsolatedSpecificMethod>();
                _isolatedSpecificMethods[hashCode] = list;
            }

            var arguments = new List<object>();
            foreach (var expression in member.Arguments)
            {
                var constant = expression as ConstantExpression;
                if (constant == null)
                {
                    throw new Exception("Contant expression expected");
                }

                arguments.Add(constant.Value);
            }

            list.Add(new IsolatedSpecificMethod(arguments.ToArray(), returnValue));
        }

        public class IsolatedMethod
        {
            public IsolatedMethod(Func<object> returnValue)
            {
                ReturnValue = returnValue;
            }

            public Func<object> ReturnValue { get; }
        }

        public class IsolatedSpecificMethod
        {
            public IsolatedSpecificMethod(object[] arguments, Func<object> returnValue)
            {
                Arguments = arguments;
                ReturnValue = returnValue;
            }

            public object[] Arguments { get; }

            public Func<object> ReturnValue { get; }
        }
    }
}
