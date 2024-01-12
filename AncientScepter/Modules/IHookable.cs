using System;
using System.Collections.Generic;
using System.Text;

namespace AncientScepter.Modules
{
    internal interface IHookable
    {
        /// <summary>
        /// Method to use and to put your IL or ON hooks into that do subscriptions.
        /// </summary>
        void AddHooks();
        /// <summary>
        /// Method to use and to put your IL or ON hooks into that unsubscribe from methods.
        /// </summary>
        void RemoveHooks();
    }
}
