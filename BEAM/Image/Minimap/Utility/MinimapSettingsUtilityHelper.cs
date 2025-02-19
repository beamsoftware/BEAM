﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using BEAM.Exceptions;
using NP.Utilities;

namespace BEAM.Image.Minimap.Utility;

public static class MinimapSettingsUtilityHelper
{
    private static List<Minimap>? _defaultMinimaps;
    private static Minimap? _currentDefault;
    private static readonly Type DefaultType = typeof(PlotMinimap);
    
    private static void GenerateMinimaps()
    {
        
        _defaultMinimaps = new List<Minimap>();
        _defaultMinimaps.AddAll(Assembly.GetExecutingAssembly().GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false } && t.IsSubclassOf(typeof(Minimap)))
            .ToList().ReplaceEveryEntry(TypeToMinimap));
        if (_currentDefault is null)
        {
            var defaults = _defaultMinimaps.Where(t => t.GetType().Equals(DefaultType)).ToList();
            if (!defaults.IsNullOrEmpty())
            {
                _currentDefault = defaults.First();
            } else
            {
                _currentDefault ??= _defaultMinimaps.First();
            }
        }
    }

    
    public static ImmutableList<Minimap> GetDefaultMinimaps()
    {
        if (_defaultMinimaps is null)
        {
            GenerateMinimaps();
        }

        return _defaultMinimaps!.ToImmutableList();
    }
    
    
    public static void SetDefaultMinimap(Minimap? newMinimapDefault)
    {
        if (newMinimapDefault is null)
        {
            return;
        }
        _currentDefault = newMinimapDefault;
    }
    
    

    public static bool ExistAny()
    {
        if(_defaultMinimaps is null) GenerateMinimaps();
        return _defaultMinimaps!.Count > 0;
    }
    

    private static Minimap TypeToMinimap(Type T) 
    {
        var result = T.GetConstructors().Where(t => t.GetParameters().Length == 0).ToList();
        if (!result.Any())
        {
            throw new ArgumentException("Input Type 'T' should have a parameterless constructor");
        }
        return (Minimap)result.First().Invoke([]);
        
    }
    public static Minimap? GetDefaultMinimap()
    {
        return !ExistAny() ? null : _currentDefault;
    }
    
    public static SettingsTransferObject<Minimap> GetDefaultClones()
    {
        Minimap? defaultClone = null;
        List<Minimap> allPossible = new List<Minimap>();
        if(_defaultMinimaps is null) GenerateMinimaps();
        foreach (var entry in _defaultMinimaps!)
        {
            Minimap clone = entry.Clone();
            if (entry.Equals(_currentDefault))
            {
                defaultClone = clone;
            }

            allPossible.Add(clone);
        }
        return new SettingsTransferObject<Minimap>(allPossible.ToImmutableList(), defaultClone);
    }
}