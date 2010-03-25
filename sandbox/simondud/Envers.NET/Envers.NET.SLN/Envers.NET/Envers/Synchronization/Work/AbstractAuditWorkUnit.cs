﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate.Engine;
using NHibernate.Envers.Configuration;

namespace NHibernate.Envers.Synchronization.Work
{
    /**
     * @author Simon Duduica, port of Envers omonyme class by Adam Warski (adam at warski dot org)
     */
    public abstract class AbstractAuditWorkUnit : IAuditWorkUnit {
	    protected readonly ISessionImplementor sessionImplementor;
        protected readonly AuditConfiguration verCfg;
        protected readonly object id;
        protected readonly String entityName;

        public Object performedData;

        protected AbstractAuditWorkUnit(ISessionImplementor sessionImplementor, String entityName, AuditConfiguration verCfg,
									    Object id) {
		    this.sessionImplementor = sessionImplementor;
            this.verCfg = verCfg;
            this.id = id;
            this.entityName = entityName;
        }

        protected void FillDataWithId(IDictionary<String, Object> data, Object revision, RevisionType revisionType) {
            AuditEntitiesConfiguration entitiesCfg = verCfg.getAuditEntCfg();

            IDictionary<String, Object> originalId = new Dictionary<String, Object>();
            originalId.Add(entitiesCfg.RevisionFieldName, revision);

            verCfg.getEntCfg()[EntityName].GetIdMapper().MapToMapFromId(originalId, id);
            data.Add(entitiesCfg.RevisionTypePropName, revisionType);
            data.Add(entitiesCfg.OriginalIdPropName, originalId);
        }

        public void Perform(ISession session, Object revisionData) {
            IDictionary<String, Object> data = GenerateData(revisionData);

            session.Save(verCfg.getAuditEntCfg().GetAuditEntityName(EntityName), data);

            SetPerformed(data);
        }

        public bool IsPerformed() {
            return performedData != null;
        }

        protected void SetPerformed(Object performedData) {
            this.performedData = performedData;
        }

        public void Undo(ISession session) {
            if (IsPerformed()) {
                session.Delete(verCfg.getAuditEntCfg().GetAuditEntityName(EntityName), performedData);
                session.Flush();
            }
        }

        #region IWorkUnitMergeDispatcher Members
        public abstract IAuditWorkUnit Dispatch(IWorkUnitMergeVisitor first);
        #endregion

        #region IWorkUnitMergeVisitor Members
        public abstract IAuditWorkUnit Merge(AddWorkUnit second);
        public abstract IAuditWorkUnit Merge(ModWorkUnit second);
        public abstract IAuditWorkUnit Merge(DelWorkUnit second);
        public abstract IAuditWorkUnit Merge(CollectionChangeWorkUnit second);
        public abstract IAuditWorkUnit Merge(FakeBidirectionalRelationWorkUnit second);
        #endregion

        #region IAuditWorkUnit Members

        public object EntityId { get; private set; }
        public string EntityName { get; private set; }
        public abstract IDictionary<String, Object> GenerateData(Object revisionData);
        public abstract bool ContainsWork();


        #endregion
    }
}
