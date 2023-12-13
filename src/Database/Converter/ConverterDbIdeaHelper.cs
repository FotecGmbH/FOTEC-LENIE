// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System;
using Database.Tables;
using Exchange.Model.Idea;

namespace Database.Converter
{
    /// <summary>
    ///     <para>Converter Helper</para>
    ///     Klasse ConverterDbIdeaHelper. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public static class ConverterDbIdeaHelper
    {
        /// <summary>
        ///     Umwandeln für Exchange
        /// </summary>
        /// <param name="dbItem"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static ExIdeaHelper ToExIdeaHelper(this TableIdeaHelper dbItem)
        {
            if (dbItem == null)
            {
                throw new ArgumentNullException(nameof(dbItem));
            }

            var exItem = new ExIdeaHelper
                         {
                             IdeaId = dbItem.TblIdeaId,

                             HasInfo = !string.IsNullOrWhiteSpace(dbItem.Info),
                             Info = dbItem.Info,

                             HasTimespan = dbItem.From.HasValue && dbItem.To.HasValue,
                             From = dbItem.From ?? DateTime.UtcNow,
                             To = dbItem.To ?? DateTime.UtcNow,
                         };

            return exItem;
        }

        /// <summary>
        ///     Umwandeln für DB
        /// </summary>
        /// <param name="exItem"></param>
        /// <param name="dbItem"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public static void ToTableHelper(this ExIdeaHelper exItem, TableIdeaHelper dbItem)
        {
            if (exItem == null)
            {
                throw new ArgumentNullException(nameof(exItem));
            }

            if (dbItem == null)
            {
                throw new ArgumentNullException(nameof(dbItem));
            }

            dbItem.Info = exItem.HasInfo ? exItem.Info : string.Empty;

            dbItem.From = exItem.HasTimespan ? exItem.From : null;
            dbItem.To = exItem.HasTimespan ? exItem.To : null;
        }

        /// <summary>
        ///     Umwandeln für Exchange
        /// </summary>
        /// <param name="dbItem"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static ExIdeaSupply ToExIdeaSupply(this TableIdeaSupply dbItem)
        {
            if (dbItem == null)
            {
                throw new ArgumentNullException(nameof(dbItem));
            }

            var exItem = new ExIdeaSupply
                         {
                             Amount = dbItem.Amount,
                             NeedId = dbItem.TblIdeaNeedId,
                         };

            if (dbItem.TblIdeaNeed != null!)
            {
                var exNeed = dbItem.TblIdeaNeed.ToExIdeaNeed();

                exItem.NeedName = exNeed.Title;
                exItem.NeedHasInfo = exNeed.HasInfo;
                exItem.NeedInfo = exNeed.Infotext;
                exItem.NeedAmount = exNeed.AmountNeed;
                exItem.NeedAmountCurrent = exNeed.AmountSupplied;
            }

            return exItem;
        }
    }
}