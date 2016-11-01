using StateMonitorControl.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StateMonitorControl.Queries
{
    /// <summary>
    /// this class hold functions for doing CRUD operations through Entity Framework  
    /// Author: Francisco F. Cardoso
    /// </summary>
    public class StateMonitorCRUD
    {
        /// <summary>
        /// retrieves specific state monitor in database
        /// </summary>
        /// <param name="id"></param>
        /// <param name="db"></param>       
        /// <returns>state monitor</returns>
        public StateMonitor selectStateMonitor(long id, STATE_MONITOR_DBEntities db)
        {
            StateMonitor stateMonitor =
            (from stateMonitors in db.StateMonitors
             join stateMonitorMetric in db.StateMonitorMetrics on stateMonitors.stateMonitorId equals stateMonitorMetric.stateMonitorId
             join state in db.States on stateMonitorMetric.stateMonitorMetricId equals state.stateMonitorMetricId
             where (stateMonitors.stateMonitorId == id)
             select stateMonitors).FirstOrDefault();

            return stateMonitor;
        }

        /// <summary>
        /// retrieves range of state monitors in database
        /// </summary>
        /// <param name="db"></param>
        /// <returns>state monitor</returns>
        public IQueryable<StateMonitor> selectStateMonitor(STATE_MONITOR_DBEntities db)
        {
            IQueryable<StateMonitor> stateMonitor =
            from stateMonitors in db.StateMonitors
            join stateMonitorMetric in db.StateMonitorMetrics on stateMonitors.stateMonitorId equals stateMonitorMetric.stateMonitorId
            join state in db.States on stateMonitorMetric.stateMonitorMetricId equals state.stateMonitorMetricId
            where stateMonitors.status == "active"
            select stateMonitors;

            return stateMonitor;
        }

        /// <summary>
        /// inserts a new state monitor in database
        /// </summary>
        /// <param name="stateMonitor"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        public void insertStateMonitor(StateMonitor stateMonitor, STATE_MONITOR_DBEntities db)
        {
            db.StateMonitors.Add(stateMonitor);
            db.SaveChanges();
        }

        /// <summary>
        /// deletes state monitor from database
        /// </summary>
        /// <param name="id"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        public void deleteStateMonitor(long id, STATE_MONITOR_DBEntities db)
        {
            //PS: Uncomment these two commented lines for changing behaviour from inactivation to real deletion
            //deleteChildren(id, db);

            StateMonitor stateMonitor =
            (from stateMonitors in db.StateMonitors
             join stateMonitorMetric in db.StateMonitorMetrics on stateMonitors.stateMonitorId equals stateMonitorMetric.stateMonitorId
             join state in db.States on stateMonitorMetric.stateMonitorMetricId equals state.stateMonitorMetricId
             where stateMonitors.stateMonitorId == id
             select stateMonitors).FirstOrDefault();

            stateMonitor.status = "inactive";
            //db.StateMonitors.Remove(stateMonitor);
            db.SaveChanges();
        }       

        /// <summary>
        /// updates state monitor from database
        /// </summary>
        /// <param name="id"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        public void updateStateMonitor(StateMonitor newStateMonitor, STATE_MONITOR_DBEntities db)
        {
            updateChildren(newStateMonitor, db);

            StateMonitor stateMonitor =
            (from stateMonitors in db.StateMonitors
             where stateMonitors.stateMonitorId == newStateMonitor.stateMonitorId
             select stateMonitors).FirstOrDefault();

            stateMonitor.thingType = newStateMonitor.thingType;
            stateMonitor.thingLvl = newStateMonitor.thingLvl;
            stateMonitor.thingName = newStateMonitor.thingName;
            stateMonitor.thingId = newStateMonitor.thingId;
            stateMonitor.status = newStateMonitor.status;

            db.SaveChanges();
        }

        private STATE_MONITOR_DBEntities updateChildren(StateMonitor newStateMonitor, STATE_MONITOR_DBEntities db)
        {
            deleteMetrics(newStateMonitor.stateMonitorId, db);
            insertMetrics(newStateMonitor, db);
            return db;
        }

        private STATE_MONITOR_DBEntities insertMetrics(StateMonitor newStateMonitor, STATE_MONITOR_DBEntities db)
        {
            foreach (StateMonitorMetric stateMonitorMetric in newStateMonitor.StateMonitorMetrics )
            {
                deleteStates(stateMonitorMetric.stateMonitorMetricId, db);
                insertStates(stateMonitorMetric, db);
                stateMonitorMetric.stateMonitorId = newStateMonitor.stateMonitorId;
                db.StateMonitorMetrics.Add(stateMonitorMetric);
            }
            return db;
        }

        private STATE_MONITOR_DBEntities insertStates(StateMonitorMetric newStateMonitorMetric, STATE_MONITOR_DBEntities db)
        {
            foreach (State state in newStateMonitorMetric.States)
            {
                state.stateMonitorMetricId = newStateMonitorMetric.stateMonitorMetricId;
                db.States.Add(state);
            }
            return db;
        }

        private STATE_MONITOR_DBEntities deleteChildren(long id, STATE_MONITOR_DBEntities db)
        {
            deleteMetrics(id, db);
            return db;
        }

        private STATE_MONITOR_DBEntities deleteMetrics(long id, STATE_MONITOR_DBEntities db)
        {
            IQueryable<StateMonitorMetric> stateMonitorMetricList =
            from stateMonitorMetrics in db.StateMonitorMetrics
            where stateMonitorMetrics.stateMonitorId == id
            select stateMonitorMetrics;

            foreach (StateMonitorMetric stateMonitorMetric in stateMonitorMetricList)
            {
                db = deleteStates(stateMonitorMetric.stateMonitorMetricId, db);
                db.StateMonitorMetrics.Remove(stateMonitorMetric);
            }

            return db;
        }

        private STATE_MONITOR_DBEntities deleteStates(long id, STATE_MONITOR_DBEntities db)
        {
            IQueryable<State> stateList =
            from states in db.States
            where states.stateMonitorMetricId == id
            select states;

            foreach (State state in stateList)
            {
                db.States.Remove(state);
            }

            return db;
        }
    }
}