using System;
using System.Collections.Generic;
using System.Text;
using System.Net;

using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace VSTSTimeTrackerAddin
{
    public class TeamFoundationWrapper
    {
        private TeamFoundationServer tfs = null; // Referência para o TFS
        private WorkItemStore store = null;
        private List<Project> projList = null;
        private List<WorkItem> workItemList = null;

        public bool LogOn(string serverName, NetworkCredential nc)
        {
            //if (tfs != null)
            //    throw new Exception("Already logged into server!");                

            tfs = new TeamFoundationServer(serverName, nc);

            try
            {
                tfs.Authenticate();
            }
            catch (Exception )
            {
                return false;
            }

            if ( tfs==null || !tfs.HasAuthenticated )
                return false;

            return true;
        }

        public bool LogOff()
        {
            tfs.Dispose();
            tfs = null;
            return true;
        }

        public List<Project> GetProjects()
        {
            if (tfs == null)
                throw new Exception("Not logged into server!");

            if ( store==null )
                store = (WorkItemStore)tfs.GetService(typeof(WorkItemStore));

            System.Collections.IEnumerator proColEnum = store.Projects.GetEnumerator();

            projList = new List<Project>();

            while (proColEnum.MoveNext())
            {
                Project proj = null;

                try
                {
                    proj = (Project)proColEnum.Current;
                    projList.Add(proj);
                }
                catch (Exception e)
                {
                    throw e;
                }
            }

            return projList;
        }

        // Essentials of Work Item Queries: 
        // http://msdn2.microsoft.com/en-us/library/bb130330(VS.80).aspx
        // 
        // Work Item Query Language (Work Item Query Reference):
        // http://msdn2.microsoft.com/en-us/library/bb130198(VS.80).aspx
        public List<WorkItem> GetProjWorkItems(string projectName)
        {
            if (tfs == null)
                throw new Exception("Not logged into server!");

            if (store == null)
                store = (WorkItemStore)tfs.GetService(typeof(WorkItemStore));

            WorkItemCollection wic = store.Query("SELECT [System.Id], [System.Title] FROM WorkItems WHERE [System.TeamProject] = '"+projectName+"'");

            System.Collections.IEnumerator wiEnum = wic.GetEnumerator();

            workItemList = new List<WorkItem>();

            while (wiEnum.MoveNext())
            {
                WorkItem wi = (WorkItem)wiEnum.Current;
                workItemList.Add(wi);
            }
            return workItemList;
        }

        public List<WorkItemType> GetProjWorkItemTypes(string projName)
        {
            List<WorkItemType> witList = new List<WorkItemType>();

            if (tfs == null)
                throw new Exception("Not logged into server!");

            if (store == null)
                store = (WorkItemStore)tfs.GetService(typeof(WorkItemStore));

            WorkItemTypeCollection witc = store.Projects[projName].WorkItemTypes;
            System.Collections.IEnumerator witcEnum = witc.GetEnumerator();

            while ( witcEnum.MoveNext() )
            {
                WorkItemType wit = (WorkItemType)witcEnum.Current;
                witList.Add(wit);
            }            
            return witList;
        }

        public bool StoreProjWorkItemCompletedWorkTime(string projName, string workItemId, TimeSpan time)
        { 
            if (tfs == null)
                throw new Exception("Not logged into server!");

            if (store == null)
                store = (WorkItemStore)tfs.GetService(typeof(WorkItemStore));

            // Get WorkItem
            WorkItem wi = store.GetWorkItem( int.Parse( workItemId ) );

            double hours = (time.TotalSeconds/60)/60;
            wi.Fields["Completed Work"].Value = hours;

            //if ( wi.Fields["Completed Work"].Value == null || 
            //        (double)wi.Fields["Completed Work"].Value == 0)
            //    wi.Fields["Completed Work"].Value = hours;
            //else
            //    wi.Fields["Completed Work"].Value = (double)wi.Fields["Completed Work"].Value + hours;

            wi.Save();
            
            return true;
        }

        public string GetProjWorkItemCompletedWorkTime(string projName, string workItemId)
        { 
            if (tfs == null)
                throw new Exception("Not logged into server!");

            if (store == null)
                store = (WorkItemStore)tfs.GetService(typeof(WorkItemStore));

            // Get WorkItem
            WorkItem wi = store.GetWorkItem(int.Parse(workItemId));

            if (wi != null && wi.Fields["Completed Work"].Value!=null)
                return ((double)wi.Fields["Completed Work"].Value).ToString();

            return null;
        }
    }
}
