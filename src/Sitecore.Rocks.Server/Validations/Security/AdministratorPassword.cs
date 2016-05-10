// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using Sitecore.Diagnostics;
using Sitecore.Security.Authentication;

namespace Sitecore.Rocks.Server.Validations.Security
{
    [Validation("Administrator password changed", "Security")]
    public class AdministratorPassword : Validation
    {
        public override bool CanCheck(string contextName)
        {
            Assert.ArgumentNotNull(contextName, nameof(contextName));

            return contextName == "Site";
        }

        public override void Check(ValidationWriter output)
        {
            Assert.ArgumentNotNull(output, nameof(output));

            bool isValid;
            try
            {
                try
                {
                    isValid = AuthenticationManager.Login("sitecore\\admin", "b", false);
                }
                catch (NullReferenceException ex)
                {
                    if (!ex.StackTrace.Contains("FormsAuthentication.SetAuthCookie"))
                    {
                        throw;
                    }

                    isValid = true;
                }
            }
            catch (Exception ex)
            {
                output.Write(SeverityLevel.Hint, "Administrator password changed", "An exception occured while checking the administrator password. The password may or may not have been changed: " + ex.Message, "The administrator password must be changed. To change it, open the User Manager in the Sitecore web client and change the password for \"sitecore\\admin\".");
                return;
            }

            if (isValid)
            {
                return;
            }

            output.Write(SeverityLevel.Error, "Administrator password changed", "The administrator password has not been changed.", "The administrator password must be changed. To change it, open the User Manager in the Sitecore web client and change the password for \"sitecore\\admin\".");
        }
    }
}
