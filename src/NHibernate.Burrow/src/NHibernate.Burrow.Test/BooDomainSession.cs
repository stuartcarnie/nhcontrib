using NHibernate.Burrow.Util.DomainSession;

namespace NHibernate.Burrow.Test {
    public class BooDomainSession : DomainSessionBase, IHasUserId {
        public string test = "test";
        private object userId;

        /// <summary>
        /// Just return the BooDomainLayer.Current as BooDomainLayer rather than DomainLayerBase
        /// </summary>
        public new static BooDomainSession Current {
            get { return DomainSessionBase.Current as BooDomainSession; }
        }

        #region IHasUserId Members

        public object UserId {
            get { return userId; }
            set { userId = value; }
        }

        #endregion
    }
}