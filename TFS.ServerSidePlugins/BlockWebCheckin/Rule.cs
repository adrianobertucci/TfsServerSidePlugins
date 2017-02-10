using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.Common;
using Microsoft.TeamFoundation.Framework.Server;
using Microsoft.TeamFoundation.VersionControl.Server;
using System.Diagnostics;

namespace BlockWebCheckin
{
    public class Rule : Microsoft.TeamFoundation.Framework.Server.ISubscriber
    {
        public string Name
        {
            get
            {
                return "TFS.ServerSidePlugins.BlockWebCheckin";
            }
        }

        public SubscriberPriority Priority
        {
            get
            {
                return SubscriberPriority.Normal;
            }
        }

        public EventNotificationStatus ProcessEvent(IVssRequestContext requestContext, NotificationType notificationType, object notificationEventArgs, out int statusCode, out string statusMessage, out ExceptionPropertyCollection properties)
        {
            statusCode = 0;
            statusMessage = string.Empty;
            properties = null;

            try
            {
               
                if (notificationType == NotificationType.DecisionPoint && notificationEventArgs is CheckinNotification)
                {
                    CheckinNotification args = notificationEventArgs as CheckinNotification;
                    //Through the temporary Workspace created by TWA, validate and block the checkin in Team Web Access.
                    if(args.CheckinType == CheckinType.Workspace)
                    { 
                        if(args.WorkspaceName != null)
                        { 
                            if (args.WorkspaceName.Contains("TMP-TfsWebCheckin"))
                            {
                                statusCode = -1;
                                statusMessage = "[TFS.ServerSidePlugins.BlockWebCheckin] Updates on Team Web Access is not allowed. Please use another editor.";
                                return EventNotificationStatus.ActionDenied;
                            }
                        }
                    }

                }

                return EventNotificationStatus.ActionPermitted;
            }
            catch (Exception ex)
            {
                statusCode = -1;
                statusMessage = "[TFS.ServerSidePlugins.BlockWebCheckin - Error] " + Name + ", details: " + ex.ToString();
                EventLog.WriteEntry("TFS Service", statusMessage, EventLogEntryType.Error);
                return EventNotificationStatus.ActionDenied;
            }
        }

        public Type[] SubscribedTypes()
        {
            return new Type[] { typeof(CheckinNotification) };
        }
    }
}
