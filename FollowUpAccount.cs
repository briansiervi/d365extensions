using System;
using System.ServiceModel;
using Microsoft.Xrm.Sdk;

namespace D365Extensions.Account
{
    public class FollowUpAccount : IPlugin
    {
        /// <summary>
        /// Create an asynchronous plug-in, registered on the  Create Message of the account table (i.e entity here).
        /// This plug-in will create a teaks activity that will remind the create (owner) of account to follow up one week later
        /// <summary>
        public void Execute(IServiceProvider serviceProvider)
        {
            ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            // Obtain the execution context from the service provider.
            IPluginExceptionContext context = (IPluginExceptionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

            // The InputParameter collection conatins all the data passed in the message request
            if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity account)
            {
                IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);
            }

            try
            {
                tracingService.Trace(account.Id.ToString());

                Entity followup = new Entity("task");
                followup["subject"] = "Send e-mail to the new customer.";
                followup["description"] = "Follow up with the customer. Check if there are any new issues thet need resolution.";
                followup["schedulestart"] = DateTime.Now.AddDays(7);
                followup["schedulesend"] = DateTime.Now.AddDays(7);
                followup["category"] = context.PrimaryEntityName;

                if (context.OutputParameter.Contains("id"))
                {
                    Guid regardingobjectid = new Guid(context.OutputParameters["id"].ToString());
                    string regardingobjectidType = "account";

                    followup["regardingobjectidType"] = new EntityReference(regardingobjectidType, regardingobjectid);

                    tracingService.Trace($"{this.GetType().Name}: Creating the task activity.");
                    service.Create(followup);
                }
            }
            catch (Exception e)
            {
                tracingService.Trace($"{this.GetType().Name} Error: {0}", ex.ToString());
                throw;
            }
        }
    }
}
