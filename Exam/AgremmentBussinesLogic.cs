using System;
using System.Collections.Generic;
using System.IdentityModel.Metadata;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using Exam;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Test.Plugin.Opportunity
{
    public class AgremmentBussinesLogic : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);
            try
            {
                Entity Target = null;
                string message = context.MessageName.ToString();
                Target = (Entity)context.InputParameters["Target"];

                EntityReference AccountER = Target.GetAttributeValue<EntityReference>("test_account");
                Entity Account = service.Retrieve(AccountER.LogicalName, AccountER.Id, new ColumnSet("primarycontactid"));
                EntityReference Contact = Account.GetAttributeValue<EntityReference>("primarycontactid");
                int AgreementType = Target.GetAttributeValue<OptionSetValue>("test_agreementtype").Value;

                if (AgreementType == (int)agrementType.Onboarding || AgreementType == (int)agrementType.NDA)
                {
                    CheckExistingAgreement(service, AccountER.Id, AgreementType);

                    if (Target.Contains("test_agreementstartdate") && Target.Contains("test_agreementenddate"))
                    {
                        UpdateOpportunities(service, Account);
                    }
                }

            }
            catch (Exception ex) { throw new InvalidPluginExecutionException(ex.Message); }
        }

        private void CheckExistingAgreement(IOrganizationService service, Guid accountId, int agreementType)
        {
            var fetchXml = $@"
                <fetch version='1.0' mapping='logical' no-lock='false' distinct='true'>
                    <entity name='test_agreement'>
                        <attribute name='test_account'/>
                        <attribute name='test_agreementtype'/>
                        <filter type='and'>
                            <condition attribute='statecode' operator='eq' value='{(int)stateCode.Active}'/>
                            <condition attribute='test_account' operator='eq' value='{accountId}'/>
                            <condition attribute='test_agreementtype' operator='eq' value='{agreementType}'/>
                        </filter>
                    </entity>
                </fetch>";

            EntityCollection agreements = service.RetrieveMultiple(new FetchExpression(fetchXml));

            if (agreements.Entities.Count > 0)
            {
                string agreementTypeName = agreementType == (int)agrementType.Onboarding ? "Onboarding" : (agreementType == (int)agrementType.NDA ? "NDA" : "Other");
                throw new InvalidPluginExecutionException($"An {agreementTypeName} Agreement has already been created!");
            }
        }

        private void UpdateOpportunities(IOrganizationService service, Entity account)
        {
            QueryExpression query = new QueryExpression("opportunity");
            query.ColumnSet = new ColumnSet("parentaccountid", "statecode");

            FilterExpression filter = new FilterExpression(LogicalOperator.And);
            ConditionExpression stateCondition = new ConditionExpression("statecode", ConditionOperator.Equal, (int)stateCode.Active);
            ConditionExpression accountCondition = new ConditionExpression("parentaccountid", ConditionOperator.Equal, account.Id);
            filter.AddCondition(stateCondition);
            filter.AddCondition(accountCondition);

            query.Criteria = filter;

            EntityCollection opportunities = service.RetrieveMultiple(query);

            foreach (var opportunity in opportunities.Entities)
            {
                Entity updatedOpportunity = new Entity(opportunity.LogicalName, opportunity.Id);
                bool tc = opportunity.GetAttributeValue<bool>("test_tcs");

                updatedOpportunity["test_tcs"] = true;
                service.Update(updatedOpportunity);
                Console.WriteLine("Updated Records" + opportunity.Id.ToString());
            }
        }
    }
}
