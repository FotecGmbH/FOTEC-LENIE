// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System.Collections.Generic;
using System.Threading.Tasks;
using Database;

namespace ConnectivityHost.Pages
{
    /// <summary>
    ///     Home Page.
    /// </summary>
    public partial class ViewHomepage
    {
        /// <summary>
        ///     Einstellungen für den Alert (modaler Dialog)
        /// </summary>
        bool _showModal;

        string _modalContent = "";
        void ModalShow() => _showModal = true;
        void ModalCancel() => _showModal = false;

        /// <summary>
        ///     Method invoked when the component is ready to start, having received its
        ///     initial parameters from its parent in the render tree.
        ///     Override this method if you will perform an asynchronous operation and
        ///     want the component to refresh when that operation is completed.
        /// </summary>
        /// <returns>A <see cref="T:System.Threading.Tasks.Task" /> representing any asynchronous operation.</returns>
        protected override async Task OnInitializedAsync()
        {
#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
            await using var db = new Db();
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task

            _lstLinks.Add(new LinkListItem("FOTEC", "https://www.fotec.at"));
            _lstLinks.Add(new LinkListItem("App", openalert: true));


            _lstRechts.Add(new LinkListItem("Interface REST", "/swagger"));
            _lstRechts.Add(new LinkListItem("Admin", "/admin"));

            await base.OnInitializedAsync().ConfigureAwait(true);
        }

        /// <summary>
        ///     Link Liste für die linke Spalte
        /// </summary>
        readonly List<LinkListItem> _lstLinks = new();

        /// <summary>
        ///     Link Liste für die rechte Spalte
        /// </summary>
        readonly List<LinkListItem> _lstRechts = new();

        /// <summary>
        ///     Fenster anzeigen
        /// </summary>
        void OpenAlert()
        {
            _modalContent = "Nicht verfügbar in der aktuellen Release.";
            ModalShow();
        }

        /// <summary>
        ///     Link Item
        /// </summary>
#pragma warning restore CA1852
        private class LinkListItem
#pragma warning disable CA1852
        {
            public LinkListItem(string info = "", string url = "", bool openalert = false)
            {
                Info = info;
                Url = url;
                OpenAlert = openalert;
            }

            #region Properties

            /// <summary>
            ///     Info
            /// </summary>
            public string Info { get; } = string.Empty;

            /// <summary>
            ///     Url.
            /// </summary>
            public string Url { get; } = string.Empty;

            /// <summary>
            ///     Alert aufnehmen.
            /// </summary>
            public bool OpenAlert { get; }

            #endregion
        }

        #region Nested Types

        #endregion
    }
}