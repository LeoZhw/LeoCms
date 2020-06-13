using System;
using System.Collections.Generic;
using System.Text;

namespace Leo.Microservice.Abstractions.Cache.Model
{
    public abstract class ConsistentHashNode
    {
        /// <summary>
        /// 主机
        /// </summary>
        /// <remarks>
        /// 	<para>创建：张宏伟</para>
        /// 	<para>日期：2016/4/2</para>
        /// </remarks>
        public string Host
        {
            get;
            set;
        }

        /// <summary>
        /// 端口
        /// </summary>
        /// <remarks>
        /// 	<para>创建：张宏伟</para>
        /// 	<para>日期：2016/4/2</para>
        /// </remarks>
        public int Port
        {
            get;
            set;
        }

        public abstract override string ToString();

        #region Equality members

        public override bool Equals(object obj)
        {
            var model = obj as ConsistentHashNode;
            if (model == null)
                return false;

            if (obj.GetType() != GetType())
                return false;

            return model.ToString() == ToString();
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        public static bool operator ==(ConsistentHashNode model1, ConsistentHashNode model2)
        {
            return Equals(model1, model2);
        }

        public static bool operator !=(ConsistentHashNode model1, ConsistentHashNode model2)
        {
            return !Equals(model1, model2);
        }
        #endregion

    }
}
