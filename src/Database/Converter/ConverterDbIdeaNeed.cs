// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System;
using System.Linq;
using Database.Tables;
using Exchange.Model.Idea;

namespace Database.Converter
{
    /// <summary>
    ///     <para>Idea Need Converter</para>
    ///     Klasse ConverterDbIdeaNeed. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public static class ConverterDbIdeaNeed
    {
        /// <summary>
        ///     Umwandeln für Exchangeklasse
        /// </summary>
        /// <param name="dbItem"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static ExIdeaNeed ToExIdeaNeed(this TableIdeaNeed dbItem)
        {
            if (dbItem == null)
            {
                throw new ArgumentNullException(nameof(dbItem));
            }

            var exItem = new ExIdeaNeed
                         {
                             IdeaId = dbItem.TblIdeaId,
                             Title = dbItem.Title,
                             AmountNeed = dbItem.Amount,
                             HasInfo = !string.IsNullOrWhiteSpace(dbItem.Info),
                             Infotext = dbItem.Info,
                         };

            if (dbItem.TblIdeaSupplies != null!)
            {
                exItem.AmountSupplied = dbItem.TblIdeaSupplies.Sum(x => x.Amount);
            }

            return exItem;
        }

        /// <summary>
        ///     Speichern in DB
        /// </summary>
        /// <param name="exItem"></param>
        /// <param name="dbItem"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public static void ToTableIdeaNeed(this ExIdeaNeed exItem, TableIdeaNeed dbItem)
        {
            if (exItem == null)
            {
                throw new ArgumentNullException(nameof(exItem));
            }

            if (dbItem == null)
            {
                throw new ArgumentNullException(nameof(dbItem));
            }

            dbItem.Title = exItem.Title;
            dbItem.Amount = exItem.AmountNeed;
            dbItem.Info = exItem.HasInfo ? exItem.Infotext : string.Empty;
        }
    }
}