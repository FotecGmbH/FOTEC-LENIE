// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System.Threading.Tasks;
using Biss.Apps.Attributes;
using Biss.Apps.Map.Base;
using Biss.Apps.Map.Component;
using Biss.Apps.Map.Helper;
using Biss.Apps.Map.Model;
using Biss.Dc.Client;
using Exchange.Model.Idea;
using Exchange.Resources;

namespace BaseApp.ViewModel.Idea
{
    /// <summary>
    ///     <para>Idee auf Karte anzeigen</para>
    ///     Klasse VmIdeaMap. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    [ViewName("ViewIdeaMap")]
    public class VmIdeaMap : VmProjectBase
    {
        /// <summary>
        ///     Design Instanz für XAML d:DataContext="{x:Static viewmodels:VmIdeaMap.DesignInstance}"
        /// </summary>
        public static VmIdeaMap DesignInstance = new VmIdeaMap();

        /// <summary>
        ///     VmIdeaMap
        /// </summary>
        public VmIdeaMap() : base(ResViewMainIdea.LblIdeaMapTitle)
        {
            SetViewProperties(true);
        }

        #region Properties

        /// <summary>
        ///     Karte
        /// </summary>
        public BissMap Map => this.BcBissMap()!.BissMap;

        #endregion

        #region Overrides

        /// <summary>
        ///     OnActivated (2) für View geladen noch nicht sichtbar
        ///     Nur einmal
        /// </summary>
        public override Task OnActivated(object? args = null)
        {
            CheckPhoneConfirmation().ConfigureAwait(true);

            Map.MapItems.Clear();

            if (args is DcListDataPoint<ExIdea> dcIdea)
            {
                PageSubTitle = dcIdea.Data.Title;
                Map.MapItems.Add(new BmPoint(dcIdea.Data.Title)
                                 {
                                     Position = dcIdea.Data.Location,
                                 });
                Map.SetCenterAndZoom(dcIdea.Data.Location, BmDistance.FromKilometers(5));
            }

            return base.OnActivated(args);
        }

        #endregion
    }
}