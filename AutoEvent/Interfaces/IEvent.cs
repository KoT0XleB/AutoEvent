using Qurre.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoEvent.Interfaces
{
    public interface IEvent
    {
        /// <summary>
        /// Название ивента в команде, при котором он вызывается. Например: ev_run [CommandName]. ДОЛЖЕН БЫТЬ ОБЯЗАТЕЛЬНО В НИЖНЕМ РЕГИСТРЕ!!!1!!!!1!
        /// </summary>
        string CommandName { get; }

        /// <summary>Название</summary>
        string Name { get; }

        /// <summary>Цвет</summary>
        string Color { get; }

        /// <summary>Описание</summary>
        string Description { get; }

        int Votes { get; set; }

        /// <summary>При запуске</summary>
        void OnStart();

        /// <summary>При окончании</summary>
        void OnStop();
    }
}
