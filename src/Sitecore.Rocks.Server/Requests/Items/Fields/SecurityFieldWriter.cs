// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Xml;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Resources;
using Sitecore.Security.AccessControl;
using Sitecore.Security.Accounts;
using Sitecore.Web.UI;

namespace Sitecore.Rocks.Server.Requests.Items.Fields
{
    public class SecurityFieldWriter : FieldWriterBase
    {
        public SecurityFieldWriter([NotNull] XmlTextWriter writer) : base(writer)
        {
            Assert.ArgumentNotNull(writer, nameof(writer));
        }

        public override void WriteField([NotNull] Item item, [NotNull] Field field)
        {
            Assert.ArgumentNotNull(item, nameof(item));
            Assert.ArgumentNotNull(field, nameof(field));

            var accessRules = item.Security.GetAccessRules();
            var accounts = accessRules.Helper.GetAccounts();

            foreach (var account in accounts)
            {
                Writer.WriteStartElement(SecurityStruct.Node.Account);
                ParseAccount(item, account, accessRules);
                Writer.WriteEndElement();
            }
        }

        private static AccessPermission GetAccessPermission(InheritancePermission permission)
        {
            switch (permission)
            {
                case InheritancePermission.Allow:
                    return AccessPermission.Allow;
                case InheritancePermission.Deny:
                    return AccessPermission.Deny;
                default:
                    return AccessPermission.NotSet;
            }
        }

        private void ParseAccount([NotNull] Item item, [NotNull] Account account, [NotNull] AccessRuleCollection accessRules)
        {
            Assert.ArgumentNotNull(item, nameof(item));
            Assert.ArgumentNotNull(account, nameof(account));
            Assert.ArgumentNotNull(accessRules, nameof(accessRules));

            var accessRights = AccessRightManager.GetAccessRights();
            var first = true;
            foreach (var accessRight in accessRights)
            {
                if (!accessRight.AppliesTo(item))
                {
                    continue;
                }

                var entityPermission = accessRules.Helper.GetExplicitAccessPermission(account, accessRight, PropagationType.Entity);
                var descendantsPermission = accessRules.Helper.GetExplicitAccessPermission(account, accessRight, PropagationType.Descendants);

                if (entityPermission == AccessPermission.NotSet && descendantsPermission == AccessPermission.NotSet)
                {
                    continue;
                }

                if (first)
                {
                    RenderAccount(account);
                    first = false;
                }

                RenderPermissions(accessRight.Title, entityPermission, descendantsPermission);
            }

            RenderInheritance(accessRules, account, first);
        }

        [NotNull]
        private static string PermissionToString(AccessPermission permission)
        {
            return permission == AccessPermission.Allow ? @"+" : (permission == AccessPermission.Deny ? @"-" : @"x");
        }

        private void RenderAccount([NotNull] Account account)
        {
            Assert.ArgumentNotNull(account, nameof(account));

            // Icon of unknown account by default.
            var icon = "Applications/16x16/unknown.png";

            // Another icon if actually a role.
            var role = account as Role;
            if (role != null)
            {
                icon = "Network/16x16/id_card.png";
            }

            // Another icon if actually a user.
            var user = account as User;
            if (user != null)
            {
                icon = StringUtil.GetString(user.Profile.Portrait, "People/16x16/user2.png");
            }

            Writer.WriteAttributeString(SecurityStruct.Attribute.Icon, Images.GetThemedImageSource(icon, ImageDimension.id16x16));
            Writer.WriteAttributeString(SecurityStruct.Attribute.Name, account.Name);
        }

        private void RenderInheritance([NotNull] AccessRuleCollection accessRules, [NotNull] Account account, bool first)
        {
            Assert.ArgumentNotNull(accessRules, nameof(accessRules));
            Assert.ArgumentNotNull(account, nameof(account));

            var entityPermission = accessRules.Helper.GetInheritanceRestriction(account, AccessRight.Any, PropagationType.Entity);
            var descendantsPermission = accessRules.Helper.GetInheritanceRestriction(account, AccessRight.Any, PropagationType.Descendants);

            if (entityPermission == InheritancePermission.NotSet && descendantsPermission == InheritancePermission.NotSet)
            {
                return;
            }

            if (first)
            {
                RenderAccount(account);
            }

            RenderPermissions("Inheritance", GetAccessPermission(entityPermission), GetAccessPermission(descendantsPermission));
        }

        private void RenderPermissions([NotNull] string title, AccessPermission entityPermission, AccessPermission descendantsPermission)
        {
            Assert.ArgumentNotNull(title, nameof(title));

            Writer.WriteStartElement(SecurityStruct.Node.Permission);

            Writer.WriteAttributeString(SecurityStruct.Attribute.Title, title);
            Writer.WriteAttributeString(SecurityStruct.Attribute.EntityPermission, PermissionToString(entityPermission));
            Writer.WriteAttributeString(SecurityStruct.Attribute.DescendantsPermission, PermissionToString(descendantsPermission));

            Writer.WriteEndElement();
        }

        private static class SecurityStruct
        {
            public static class Attribute
            {
                public const string DescendantsPermission = @"d";

                public const string EntityPermission = @"e";

                public const string Icon = @"i";

                public const string Name = @"n";

                public const string Title = @"t";
            }

            public static class Node
            {
                public const string Account = @"a";

                public const string Permission = @"p";
            }
        }
    }
}
