namespace Owin.Scim.Model.Users
{
    using System.ComponentModel;

    [Description("Enterprise user.")]
    public class EnterpriseUserExtension : ResourceExtension
    {
        public string EmployeeNumber { get; set; }

        public string CostCenter { get; set; }

        public string Organization { get; set; }

        public string Division { get; set; }

        public string Department { get; set; }

        public Manager Manager { get; set; }

        protected internal override string SchemaIdentifier
        {
            get { return ScimConstants.Schemas.UserEnterprise; }
        }

        public override int CalculateVersion()
        {
            return new
            {
                EmployeeNumber,
                CostCenter,
                Organization,
                Division,
                Department,
                Manager?.Value
            }.GetHashCode();
        }
    }
}