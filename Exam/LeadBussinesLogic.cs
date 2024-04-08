using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Exam
{
    public class LeadBusinessLogic : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

            try
            {
                Entity target = null;
                int stage = context.Stage;
                target = (Entity)context.InputParameters["Target"];

                //automatically append the date after the Topic name
                string topic = target.GetAttributeValue<string>("subject");
                topic += " " + DateTime.UtcNow.ToString("dd/MM/yyyy");

                if (stage == (int)Stage.PostOperation)
                {
                    Entity task = new Entity("task");
                    task["subject"] = "Follow Up";
                    task["regardingobjectid"] = new EntityReference(target.LogicalName, target.Id);
                    service.Create(task);
                    target["subject"] = topic;
                    service.Update(target);
                }
                
            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException(ex.Message);
            }
        }
    }
}