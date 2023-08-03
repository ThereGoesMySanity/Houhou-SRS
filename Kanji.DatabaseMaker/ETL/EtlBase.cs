using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kanji.DatabaseMaker
{
    abstract class EtlBase
    {
        #region Constructors

        public EtlBase()
        {
            
        }

        #endregion

        #region Methods

        /// <summary>
        /// Executes the ETL operation.
        /// </summary>
        public abstract Task ExecuteAsync();

        #endregion
    }
}
