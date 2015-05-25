using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace FluentAssertions.Equivalency.Selection
{
    [Obsolete]
    internal class ObsoleteSelectionRuleAdapter : IMemberSelectionRule
    {
        private readonly ISelectionRule obsoleteSelectionRule;

        public ObsoleteSelectionRuleAdapter(ISelectionRule obsoleteSelectionRule)
        {
            this.obsoleteSelectionRule = obsoleteSelectionRule;
        }

        public bool IncludesMembers
        {
            get { return false; }
        }

        public IEnumerable<SelectedMemberInfo> SelectMembers(IEnumerable<SelectedMemberInfo> selectedMembers, ISubjectInfo context, IEquivalencyAssertionOptions config)
        {
            IEnumerable<PropertyInfo> propertyInfos =
                selectedMembers.OfType<PropertySelectedMemberInfo>().Select(info => info.PropertyInfo);

            return obsoleteSelectionRule.SelectProperties(propertyInfos, context).Select(SelectedMemberInfo.Create);
        }

        public override string ToString()
        {
            return obsoleteSelectionRule.ToString();
        }
    }
}