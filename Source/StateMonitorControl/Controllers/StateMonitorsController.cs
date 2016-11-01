using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using StateMonitorControl.Models;
using System.Web.Http.Cors;
using log4net;
using StateMonitorControl.Queries;

namespace StateMonitorControl.Controllers
{
    /// <summary>
    /// This class has the objective of managing the resource 'State Monitors'
    /// Author: Francisco F. Cardoso
    /// </summary>      
    /// 
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class StateMonitorsController : ApiController
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private STATE_MONITOR_DBEntities db = new STATE_MONITOR_DBEntities();
        StateMonitorCRUD smCrud = new StateMonitorCRUD();

        // GET: api/StateMonitors
        /// <summary>
        /// Get all the registered state monitors 
        /// </summary>
        /// <returns>JSON with complete list of active state monitors</returns>
        public IQueryable<StateMonitor> GetStateMonitors()
        {
            return smCrud.selectStateMonitor(db);
        }

        // GET: api/StateMonitors/5
        /// <summary>
        ///  Get a specific state monitor
        /// </summary>
        /// <param name="id"></param>
        /// <returns>JSON with details of state monitor</returns>
        [ResponseType(typeof(StateMonitor))]
        public IHttpActionResult GetStateMonitor(long id)
        {
            StateMonitor stateMonitor = smCrud.selectStateMonitor(id,db);

            if (stateMonitor == null)
            {
                return NotFound();
            }

            return Ok(stateMonitor);

        }

        // PUT: api/StateMonitors/5
        /// <summary>
        /// Update specific state monitor information.
        /// Add/Remove metrics to state monitor.
        /// Add/Remove states to metrics.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="stateMonitor"></param>
        /// <returns>Status code</returns>
        [ResponseType(typeof(void))]
        public IHttpActionResult PutStateMonitor(long id, StateMonitor stateMonitor)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != stateMonitor.stateMonitorId)
            {
                return BadRequest();
            }

            try
            {
                smCrud.updateStateMonitor(stateMonitor, db);

            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StateMonitorExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.OK);
        }

        // POST: api/StateMonitors
        /// <summary>
        /// Insert new state monitor
        /// </summary>
        /// <param name="stateMonitor"></param>
        /// <returns>JSON of just created state monitor</returns>
        [ResponseType(typeof(StateMonitor))]
        public IHttpActionResult PostStateMonitor(StateMonitor stateMonitor)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            smCrud.insertStateMonitor(stateMonitor, db);

            return CreatedAtRoute("DefaultApi", new { id = stateMonitor.stateMonitorId }, stateMonitor);
        }

        // DELETE: api/StateMonitors/5
        /// <summary>
        /// Make specific state monitor inactive
        /// </summary>
        /// <param name="id"></param>
        /// <returns>JSON of just updated state monitor</returns>
        [ResponseType(typeof(StateMonitor))]
        public IHttpActionResult DeleteStateMonitor(long id)
        {
            StateMonitor stateMonitor = db.StateMonitors.Find(id);
            if (stateMonitor == null)
            {
                return NotFound();
            }

            smCrud.deleteStateMonitor(id, db);
            return Ok(stateMonitor);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool StateMonitorExists(long id)
        {
            return db.StateMonitors.Count(e => e.stateMonitorId == id) > 0;
        }
    }
}