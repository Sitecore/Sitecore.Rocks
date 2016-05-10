// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;

namespace Sitecore.Rocks.Data
{
    public class Validator
    {
        public Guid FieldId { get; set; }

        public ValidatorResult Result { get; set; }

        public string Text { get; set; }

        public Guid ValidatorId { get; set; }
    }
}
