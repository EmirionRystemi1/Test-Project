using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.ServiceModel;

namespace Exam
{
    public class WorkorderPlugin : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            ITracingService tracingService =
            (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            IPluginExecutionContext context = (IPluginExecutionContext)
                serviceProvider.GetService(typeof(IPluginExecutionContext));

            if (context.InputParameters.Contains("Target") &&
                context.InputParameters["Target"] is Entity)
            {
                Entity Target = (Entity)context.InputParameters["Target"];

                IOrganizationServiceFactory serviceFactory =
                    (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

                try
                {
                    ColumnSet allColumns = new ColumnSet(true);
                    EntityReference AgentER = Target.GetAttributeValue<EntityReference>("test_assignedagent");
                    Entity AgentEnt = service.Retrieve(AgentER.LogicalName, AgentER.Id, allColumns);

                    string AgentName = AgentEnt.GetAttributeValue<string>("test_name");
                    bool WorksOnMonday = AgentEnt.GetAttributeValue<bool>("test_isscheduledmonday");
                    bool WorksOnTuesday = AgentEnt.GetAttributeValue<bool>("test_isscheduledtuesday");
                    bool WorksOnWednesday = AgentEnt.GetAttributeValue<bool>("test_isscheduledwednesday");
                    bool WorksOnThursday = AgentEnt.GetAttributeValue<bool>("test_isscheduledthursday");
                    bool WorksOnFriday = AgentEnt.GetAttributeValue<bool>("test_isscheduledfriday");
                    int scheduleDay = Target.GetAttributeValue<OptionSetValue>("test_scheduledon").Value;

                    bool scheduledOnMatches = true;

                    switch (scheduleDay)
                    {
                        case (int)dayOfSchedule.Monday:
                            scheduledOnMatches = WorksOnMonday;
                            break;
                        case (int)dayOfSchedule.Tuesday:
                            scheduledOnMatches = WorksOnTuesday;
                            break;
                        case (int)dayOfSchedule.Wednesday:
                            scheduledOnMatches = WorksOnWednesday;
                            break;
                        case (int)dayOfSchedule.Thursday:
                            scheduledOnMatches = WorksOnThursday;
                            break;
                        case (int)dayOfSchedule.Friday:
                            scheduledOnMatches = WorksOnFriday;
                            break;
                    }
                    if (scheduledOnMatches != true)
                    {
                        throw new InvalidPluginExecutionException("Agent " + AgentName + " isn't available on that day");
                    }
                }

                catch (FaultException<OrganizationServiceFault> ex)
                {
                    throw new InvalidPluginExecutionException("An error occurred in FollowUpPlugin.", ex);
                }

                catch (Exception ex)
                {
                    tracingService.Trace("FollowUpPlugin: {0}", ex.ToString());
                    throw;
                }
            }

        }
    }
}