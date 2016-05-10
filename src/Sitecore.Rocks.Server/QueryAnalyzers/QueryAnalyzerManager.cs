// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using Sitecore.Data.Query;
using Sitecore.Diagnostics;
using Sitecore.Exceptions;
using Sitecore.Rocks.Server.Extensibility;
using Sitecore.Rocks.Server.QueryAnalyzers.Functions;
using Sitecore.Rocks.Server.QueryAnalyzers.Opcodes;
using Sitecore.Rocks.Server.QueryAnalyzers.Parsers;
using Sitecore.Rocks.Server.QueryAnalyzers.WhereHandlers;

namespace Sitecore.Rocks.Server.QueryAnalyzers
{
    [ExtensibilityInitialization(PreInit = "Clear")]
    public static class QueryAnalyzerManager
    {
        private static readonly List<FunctionDescriptor> functionDescriptors = new List<FunctionDescriptor>();

        private static readonly List<ImportSource> importSources = new List<ImportSource>();

        private static readonly List<Keyword> keywords = new List<Keyword>();

        private static readonly List<ReservedWord> reservedWords = new List<ReservedWord>();

        private static readonly List<WhereHandler> whereHandlers = new List<WhereHandler>();

        [NotNull]
        public static IEnumerable<FunctionDescriptor> Functions
        {
            get { return functionDescriptors; }
        }

        [NotNull]
        public static IEnumerable<ImportSource> ImportSources
        {
            get { return importSources; }
        }

        [NotNull]
        public static IEnumerable<Keyword> Keywords
        {
            get { return keywords; }
        }

        [NotNull]
        public static IEnumerable<ReservedWord> ReservedWords
        {
            get { return reservedWords; }
        }

        [NotNull]
        public static IEnumerable<WhereHandler> WhereHandlers
        {
            get { return whereHandlers; }
        }

        public static void Clear()
        {
            keywords.Clear();
            whereHandlers.Clear();
            reservedWords.Clear();
        }

        [CanBeNull]
        public static Opcode GetFrom([NotNull] Parser parser, int tokenType)
        {
            Assert.ArgumentNotNull(parser, nameof(parser));

            foreach (var whereHandler in whereHandlers)
            {
                if (whereHandler.Attribute.TokenType != tokenType)
                {
                    continue;
                }

                var constructor = whereHandler.Type.GetConstructor(Type.EmptyTypes);
                if (constructor == null)
                {
                    throw new QueryException(string.Format("Constructor for From clause '{0}' not found", whereHandler.Attribute.Word));
                }

                var k = constructor.Invoke(null) as IWhereHandler;
                if (k == null)
                {
                    throw new QueryException(string.Format("Could not instantiate From clause '{0}'", whereHandler.Attribute.Word));
                }

                return k.Parse(parser);
            }

            /*
      foreach (var databaseName in Factory.GetDatabaseNames())
      {
        if (string.Compare(databaseName, parser.Token.Value, StringComparison.InvariantCultureIgnoreCase) != 0)
        {
          continue;
        }

        var k = new DatabaseWhereHandler(databaseName);

        return k.Parse(parser);
      }
      */

            return null;
        }

        [CanBeNull]
        public static IImportSource GetImportSource([NotNull] Parser parser, int tokenType)
        {
            Assert.ArgumentNotNull(parser, nameof(parser));

            foreach (var importSource in importSources)
            {
                if (importSource.Attribute.TokenType != tokenType)
                {
                    continue;
                }

                var constructor = importSource.Type.GetConstructor(Type.EmptyTypes);
                if (constructor == null)
                {
                    throw new QueryException(string.Format("Constructor for import source '{0}' not found", importSource.Attribute.Word));
                }

                var k = constructor.Invoke(null) as IImportSource;
                if (k == null)
                {
                    throw new QueryException(string.Format("Could not instantiate import source '{0}'", importSource.Attribute.Word));
                }

                k.Parse(parser);

                return k;
            }

            return null;
        }

        [CanBeNull]
        public static Opcode GetKeyword([NotNull] Parser parser, int tokenType)
        {
            Assert.ArgumentNotNull(parser, nameof(parser));

            if (tokenType == TokenType.Minus)
            {
                return ParseComment(parser);
            }

            foreach (var keyword in keywords)
            {
                if (keyword.Attribute.TokenType != tokenType)
                {
                    continue;
                }

                var constructor = keyword.Type.GetConstructor(Type.EmptyTypes);
                if (constructor == null)
                {
                    throw new QueryException(string.Format("Constructor for keyword '{0}' not found", keyword.Attribute.Keyword));
                }

                var k = constructor.Invoke(null) as IQueryAnalyzerKeyword;
                if (k == null)
                {
                    throw new QueryException(string.Format("Could not instantiate keyword '{0}'", keyword.Attribute.Keyword));
                }

                return k.Parse(parser);
            }

            return null;
        }

        public static int GetTokenType([NotNull] string value)
        {
            Assert.ArgumentNotNull(value, nameof(value));

            foreach (var keyword in keywords)
            {
                if (string.Compare(keyword.Attribute.Keyword, value, StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    return keyword.Attribute.TokenType;
                }
            }

            foreach (var reservedWord in reservedWords)
            {
                if (string.Compare(reservedWord.Attribute.Word, value, StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    return reservedWord.Attribute.TokenType;
                }
            }

            foreach (var whereHandler in whereHandlers)
            {
                if (string.Compare(whereHandler.Attribute.Word, value, StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    return whereHandler.Attribute.TokenType;
                }
            }

            foreach (var importSource in importSources)
            {
                if (string.Compare(importSource.Attribute.Word, value, StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    return importSource.Attribute.TokenType;
                }
            }

            return -1;
        }

        public static void LoadImportSource([NotNull] Type type, [NotNull] ImportSourceAttribute importSourceAttribute)
        {
            Assert.ArgumentNotNull(type, nameof(type));
            Assert.ArgumentNotNull(importSourceAttribute, nameof(importSourceAttribute));

            var where = new ImportSource
            {
                Type = type,
                Attribute = importSourceAttribute
            };

            importSources.Add(where);
        }

        public static void LoadKeyword([NotNull] Type type, [NotNull] KeywordAttribute keywordAttribute)
        {
            Assert.ArgumentNotNull(type, nameof(type));
            Assert.ArgumentNotNull(keywordAttribute, nameof(keywordAttribute));

            var keyword = new Keyword
            {
                Type = type,
                Attribute = keywordAttribute
            };

            keywords.Add(keyword);
        }

        public static void LoadReservedWord([NotNull] ReservedWordAttribute reservedWordAttribute)
        {
            Assert.ArgumentNotNull(reservedWordAttribute, nameof(reservedWordAttribute));

            var reservedWord = new ReservedWord
            {
                Attribute = reservedWordAttribute
            };

            reservedWords.Add(reservedWord);
        }

        public static void LoadType([NotNull] Type type, [NotNull] FunctionAttribute functionAttribute)
        {
            Assert.ArgumentNotNull(type, nameof(type));
            Assert.ArgumentNotNull(functionAttribute, nameof(functionAttribute));

            var instance = Activator.CreateInstance(type) as IFunction;

            var functionDescriptor = new FunctionDescriptor(instance, functionAttribute);

            functionDescriptors.Add(functionDescriptor);
        }

        public static void LoadWhereHandler([NotNull] Type type, [NotNull] WhereHandlerAttribute whereHandlerAttribute)
        {
            Assert.ArgumentNotNull(type, nameof(type));
            Assert.ArgumentNotNull(whereHandlerAttribute, nameof(whereHandlerAttribute));

            var where = new WhereHandler
            {
                Type = type,
                Attribute = whereHandlerAttribute
            };

            whereHandlers.Add(where);
        }

        private static Opcode ParseComment(Parser parser)
        {
            parser.Match();
            parser.Match(TokenType.Minus, "-- comment expected.");

            var comment = parser.DoGetComment();

            return new Comment(comment);
        }

        public class FunctionDescriptor
        {
            public FunctionDescriptor(IFunction function, FunctionAttribute attribute)
            {
                Attribute = attribute;
                Function = function;
            }

            [NotNull]
            public FunctionAttribute Attribute { get; private set; }

            [NotNull]
            public IFunction Function { get; private set; }
        }

        public class ImportSource
        {
            [NotNull]
            public ImportSourceAttribute Attribute { get; set; }

            [NotNull]
            public Type Type { get; set; }
        }

        public class Keyword
        {
            [NotNull]
            public KeywordAttribute Attribute { get; set; }

            [NotNull]
            public Type Type { get; set; }
        }

        public class ReservedWord
        {
            [NotNull]
            public ReservedWordAttribute Attribute { get; set; }
        }

        public class WhereHandler
        {
            [NotNull]
            public WhereHandlerAttribute Attribute { get; set; }

            [NotNull]
            public Type Type { get; set; }
        }
    }
}
