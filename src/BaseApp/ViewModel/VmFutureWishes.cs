// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System;
using System.Threading.Tasks;
using Biss.Apps.Attributes;
using Biss.Apps.Interfaces;
using Biss.Apps.ViewModel;
using Biss.Dc.Client;
using Exchange.Model.FutureWishes;
using Exchange.Resources;

namespace BaseApp.ViewModel
{
    /// <summary>
    ///     <para>Viewmodel für Weiterentwicklung</para>
    ///     Klasse VmFutureWishes. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    [ViewName("ViewFutureWishes")]
    public class VmFutureWishes : VmProjectBase
    {
        /// <summary>
        ///     Design Instanz für XAML d:DataContext="{x:Static viewmodels:VmFutureWishes.DesignInstance}"
        /// </summary>
        public static VmFutureWishes DesignInstance = new VmFutureWishes();

        /// <summary>
        ///     Design Instanz für XAML d:DataContext="{x:Static viewmodels:VmFutureWishes.DesignInstanceWish}"
        /// </summary>
        public static DcListDataPoint<ExFutureWish> DesignInstanceWish = new DcListDataPoint<ExFutureWish>(new ExFutureWish());

        /// <summary>
        ///     VmFutureWishes
        /// </summary>
        public VmFutureWishes() : base(ResViewFutureWishes.LblTitle, ResViewFutureWishes.LblSubTitle)
        {
            SetViewProperties();
        }

        #region Overrides

        /// <summary>
        ///     OnActivated (2) für View geladen noch nicht sichtbar
        ///     Nur einmal
        /// </summary>
        public override async Task OnActivated(object? args = null)
        {
            if (!await CheckConnected().ConfigureAwait(true))
            {
                GCmdHome.IsSelected = true;
                return;
            }

            await CheckPhoneConfirmation().ConfigureAwait(true);

            View.BusySet();

#pragma warning disable CS0618 // Type or member is obsolete
            await Dc.DcExFutureWishes.WaitDataFromServerAsync(reload: true).ConfigureAwait(true);
#pragma warning restore CS0618 // Type or member is obsolete

            View.BusyClear();

            await base.OnActivated(args).ConfigureAwait(false);
        }

        #endregion

        #region Commands

        /// <summary>
        ///     Command um den Link im Browser zu öffnen
        /// </summary>
        public VmCommand CmdOpenLink { get; set; } = null!;

        /// <summary>
        ///     Command um den Liked Status zu ändern
        /// </summary>
        public VmCommand CmdToggleLiked { get; set; } = null!;

        /// <summary>
        ///     Commands Initialisieren (aufruf im Kostruktor von VmBase)
        /// </summary>
        protected override void InitializeCommands()
        {
            CmdOpenLink = new VmCommand("", async p =>
            {
                if (!await CheckConnected().ConfigureAwait(true))
                {
                    return;
                }

                if (p is DcListDataPoint<ExFutureWish> wish)
                {
                    await Open.Browser(wish.Data.Link).ConfigureAwait(true);

                    var result = await MsgBox.Show(
                            ResViewFutureWishes.MessageboxText.Replace(
                                ResViewFutureWishes.MessageBoxFunctionPlaceholder, wish.Data.Title, StringComparison.InvariantCulture),
                            string.Empty,
                            ResViewFutureWishes.MessageboxConfirm, VmMessageBoxResult.Ok,
                            ResViewFutureWishes.MessageBoxCancel, VmMessageBoxResult.Cancel)
                        .ConfigureAwait(true);

                    wish.Data.Liked = result == VmMessageBoxResult.Ok;
                    await Dc.DcExFutureWishes.StoreAll().ConfigureAwait(false);
                }
            });

            CmdToggleLiked = new VmCommand("", async q =>
            {
                if (!await CheckConnected().ConfigureAwait(true))
                {
                    return;
                }

                if (q is DcListDataPoint<ExFutureWish> wish)
                {
                    wish.Data.Liked = !wish.Data.Liked;
                    await Dc.DcExFutureWishes.StoreAll().ConfigureAwait(false);
                }
            });
        }

        #endregion
    }
}