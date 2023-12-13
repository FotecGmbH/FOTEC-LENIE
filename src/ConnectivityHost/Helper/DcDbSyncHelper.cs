// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Biss.Dc.Core;

namespace ConnectivityHost.Helper;

/// <summary>
///     <para>Hiflsklasse für Sync</para>
///     Klasse DcDbSyncHelper. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
/// </summary>
public class DcDbSyncHelper<T1>
    where T1 : IDcChangeTracking
{
    private readonly IQueryable<T1> _queryable;

    /// <summary>
    ///     Hiflsklasse für Sync
    /// </summary>
    /// <param name="queryable">Die Tabelle die abgeglichen werden soll. Zb. db.TblSampleData.AsNoTracking()</param>
    /// <exception cref="ArgumentNullException"></exception>
    public DcDbSyncHelper(IQueryable<T1> queryable)
    {
        if (queryable == null!)
        {
            throw new ArgumentNullException($"[DcDbSyncHelper](DcDbSyncHelper): {nameof(queryable)}");
        }

        _queryable = queryable;
    }

    /// <summary>
    ///     Sync Daten Abgleich
    /// </summary>
    /// <param name="current">Aktuelle Datensätze am Client</param>
    /// <param name="filter">Filter für auslesen der Daten aus dem queryable - oder bei NULL alle Daten aus dem queryable</param>
    /// <param name="selector">Optionaler Selector falls die DataVersion aus mehreren Tabellen-DataVersions besteht</param>
    /// <returns>Liste der zu entfernenden Datensätze für den Client und das neue queryable für die modifizierten Datensätzen</returns>
    public (List<long> intemsToRemove, IQueryable<T1>? modifiedElementsDb) GetSyncData(DcListSyncData current, Expression<Func<T1, bool>> filter = null!, Expression<Func<T1, DcSyncElement>> selector = null!)
    {
        if (current == null!)
        {
            throw new ArgumentNullException($"[DcDbSyncHelper]({nameof(GetSyncData)}): {nameof(current)}");
        }

        //Alle Id's 
        //Expression<Func<TSource, TResult>> selector
        List<DcSyncElement> elementsDb;
        if (filter == null!)
        {
            if (selector == null!)
            {
                elementsDb = _queryable
                    .Select(s => new DcSyncElement
                                 {
                                     DataVersion = s.DataVersion,
                                     Id = s.Id,
                                 }).ToList();
            }
            else
            {
                elementsDb = _queryable.Select(selector).ToList();
            }
        }
        else
        {
            if (selector == null!)
            {
                elementsDb = _queryable.Where(filter)
                    .Select(s => new DcSyncElement
                                 {
                                     DataVersion = s.DataVersion,
                                     Id = s.Id,
                                 }).ToList();
            }
            else
            {
                elementsDb = _queryable.Where(filter).Select(selector).ToList();
            }
        }


        //Gelöschte User (Abgleich welche in der DB nicht mehr existieren)
        var itemsToRemoveOnClient = current.CurrentListEntries
            .Select(s => s.Id)
            .Except(elementsDb.Select(s => s.Id))
            .ToList();


        //Prüfen ob sich Datensätze geändert haben
        if (current.CurrentListEntries != null! && current.CurrentListEntries.Count > 0)
        {
            foreach (var element in current.CurrentListEntries)
            {
                var elementNotChanged = elementsDb.FirstOrDefault(f => f == element);

                if (!Equals(elementNotChanged, null))
                {
                    elementsDb.Remove(elementNotChanged);
                }
            }
        }

        //Queryable der modifizierten Datensätze
        if (elementsDb.Count > 0)
        {
            var itemIds = elementsDb.Select(s => s.Id).ToList();
            var newQuery = _queryable.Where(w => itemIds.Contains(w.Id));
            return (itemsToRemoveOnClient, newQuery);
        }

        return (itemsToRemoveOnClient, null);
    }
}