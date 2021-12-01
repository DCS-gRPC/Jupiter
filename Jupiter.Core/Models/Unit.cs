﻿using System;
using RurouniJones.Jupiter.Core.ViewModels.Commands;

namespace RurouniJones.Jupiter.Core.Models
{
    public class Unit : ModelBase
    {
        private Location _location;
        public Location Location
        {
            get => _location;
            set
            {
                var lat = value.Latitude;
                var lon = value.Longitude;
                var alt = value.Altitude;
                var loc = new Location(lat, lon, alt);
                SetProperty(ref _location, loc);
            }
        }

        private string _name;
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }


        private string _player;
        public string Player
        {
            get => _player;
            set => SetProperty(ref _player, value);
        }

        private uint _id;
        public uint Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        private string _groupName;
        public string GroupName
        {
            get => _groupName;
            set => SetProperty(ref _groupName, value);
        }


        private int _coalition;
        public int Coalition
        {
            get => _coalition;
            set => SetProperty(ref _coalition, value);
        }

        private string _pilot;
        public string Pilot
        {
            get => _pilot;
            set => SetProperty(ref _pilot, value);
        }

        private string _callsign;
        public string Callsign
        {
            get => _callsign;
            set => SetProperty(ref _callsign, value);
        }

        private string _type;
        public string Type
        {
            get => _type;
            set => SetProperty(ref _type, value);
        }

        private Symbology.Context _context;
        public Symbology.Context Context
        {
            get => _context;
            set => SetProperty(ref _context, value);
        }

        private Symbology.StandardIdentity _standardIdentity;
        public Symbology.StandardIdentity StandardIdentity
        {
            get => _standardIdentity;
            set => SetProperty(ref _standardIdentity, value);
        }

        private Symbology.SymbolSet _symbolSet;
        public Symbology.SymbolSet SymbolSet
        {
            get => _symbolSet;
            set => SetProperty(ref _symbolSet, value);
        }

        private ulong _milStd2525dCode;
        public ulong MilStd2525dCode
        {
            get => _milStd2525dCode; // _milStd2525dCode;
            set => SetProperty(ref _milStd2525dCode, value);
        }

        public EnableRadarEmissionCommand EnableRadarEmissionCommand { get; }
        public DisableRadarEmissionCommand DisableRadarEmissionCommand { get; }
        public PopSmokeCommand PopSmokeCommand { get; }
        public LaunchFlareCommand LaunchFlareCommand { get; }

        public Unit()
        {
            EnableRadarEmissionCommand = new EnableRadarEmissionCommand();
            DisableRadarEmissionCommand = new DisableRadarEmissionCommand();
            PopSmokeCommand = new PopSmokeCommand();
            LaunchFlareCommand = new LaunchFlareCommand();
        }

        public override string ToString()
        {
            return $"{nameof(Location)}: {Location}, {nameof(Name)}: {Name}, {nameof(Id)}: {Id},  {nameof(GroupName)}: {GroupName}, {nameof(Coalition)}: {Coalition}, {nameof(Pilot)}: {Pilot}, {nameof(Type)}: {Type}, {nameof(MilStd2525dCode)}: {MilStd2525dCode}";
        }
    }
}
