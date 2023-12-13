// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Biss.Interfaces;
using PropertyChanged;

namespace Exchange.Model.Idea
{
    /// <summary>
    ///     <para>Helfer bei Idee</para>
    ///     Klasse ExIdeaHelper. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class ExIdeaHelper : IBissModel
    {
        #region Properties

        /// <summary>
        ///     Ideen Id
        /// </summary>
        public long IdeaId { get; set; }

        /// <summary>
        ///     User ID
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        ///     Username
        /// </summary>
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        ///     Userbild
        /// </summary>
        public string UserImage { get; set; } = string.Empty;

        /// <summary>
        ///     Hat Zeitraum angegeben
        /// </summary>
        public bool HasTimespan { get; set; }

        /// <summary>
        ///     hilft ab
        /// </summary>
        public DateTime From { get; set; } = DateTime.Now;

        /// <summary>
        ///     hilft bis
        /// </summary>
        public DateTime To { get; set; } = DateTime.Now;

        /// <summary>
        ///     Hat Infotext angegeben
        /// </summary>
        public bool HasInfo { get; set; }

        /// <summary>
        ///     Infotext des Helfers
        /// </summary>
        public string Info { get; set; } = string.Empty;

        /// <summary>
        ///     Dinge die der User zur Verfügung stellt
        /// </summary>
        public List<ExIdeaSupply> Supplies { get; set; } = new List<ExIdeaSupply>();

        /// <summary>
        ///     Supplies, wo wirklich unterstützt wird
        /// </summary>
        [DependsOn(nameof(Supplies))]
        public List<ExIdeaSupply> SuppliesUi => Supplies.Where(x => x.Amount > 0).ToList();

        /// <summary>
        ///     Kann aktueller User Editieren
        /// </summary>
        public bool CanEdit { get; set; }

        /// <summary>
        ///     Kann aktueller User löschen
        /// </summary>
        public bool CanDelete { get; set; }

        /// <summary>
        ///     Ist der aktuelle User
        /// </summary>
        public bool IsMine { get; set; }

        /// <summary>
        ///     Id für privaten Chat mit ersteller
        /// </summary>
        public long? PrivateChatId { get; set; }

        #endregion

        #region Interface Implementations

#pragma warning disable CS0067
#pragma warning disable CS0414
        /// <inheritdoc />
        public event PropertyChangedEventHandler PropertyChanged = null!;
#pragma warning restore CS0067
#pragma warning restore CS0414

        #endregion
    }
}