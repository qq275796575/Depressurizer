﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Rallion;


namespace Depressurizer {
    class TESTER {
        public static void GetLists( GameList list ) {

            string packageInfoPath = @"D:\Steam\appcache\packageinfo.vdf";
            string localConfigPath = @"D:\Steam\userdata\29476466\config\localconfig.vdf";

            StreamWriter writeNonIntPackage = new StreamWriter( "NonIntPackage.txt", false );
            StreamWriter writeInDbNotGame = new StreamWriter( "InDB_NotGame.txt", false );
            StreamWriter writeNotInDB = new StreamWriter( "NotInDB.txt", false );
            StreamWriter writeNoPackageForLicense = new StreamWriter( "NoPackageFound.txt", false );
            StreamWriter writeAllPackageApps = new StreamWriter( "AllPackageApps.txt", false );

            Dictionary<int, PackageInfo> packages = PackageInfo.LoadPackages( packageInfoPath );

            VdfFileNode vdfFile = VdfFileNode.LoadFromText( new StreamReader( localConfigPath ) );

            VdfFileNode licensesNode = vdfFile.GetNodeAt( new string[] {"UserLocalConfigStore","Licenses"}, false );

            List<int> ownedPackageIds = new List<int>();

            foreach( string key in licensesNode.NodeArray.Keys ) {
                int id;
                if( int.TryParse( key, out id ) ) {
                    ownedPackageIds.Add( id );
                } else {
                    writeNonIntPackage.WriteLine( key );
                }
            }

            Dictionary<int, AppInfo> appList = AppInfo.LoadApps( @"D:\Steam\appcache\appinfo.vdf" );

            SortedSet<int> appIds = new SortedSet<int>();
            foreach( int packageId in ownedPackageIds ) {
                if( packageId == 0 ) continue;

                if( packages.ContainsKey( packageId ) ) {
                    PackageInfo package = packages[packageId];
                    if( package.IsExpired ) {
                        continue;
                    }
                    foreach( int appId in package.appIds ) {
                        if( appList.ContainsKey( appId ) ) {
                            if( appList[appId].type.Equals( "Game", StringComparison.OrdinalIgnoreCase )) {
                                appIds.Add( appId );
                                writeAllPackageApps.WriteLine( "App {0} from Package {1}", appId, packageId );
                            } else {
                                writeInDbNotGame.WriteLine( GetGameString( (int)appId, appList ) ); // Game is in database, but isn't a game
                            }
                        } else {
                            writeNotInDB.WriteLine( appId );
                        }
                    }
                } else {
                    writeNoPackageForLicense.WriteLine( string.Format( "Package: {0}", packageId ) );
                }
                
            }

            writeInDbNotGame.Close();
            writeNonIntPackage.Close();
            writeNoPackageForLicense.Close();
            writeNotInDB.Close();
            writeAllPackageApps.Close();

            StreamWriter writeInOldNotNew = new StreamWriter( "InOldNotNew.txt", false );
            StreamWriter writeInNewNotOld = new StreamWriter( "InNewNotOld.txt", false );
            StreamWriter writeInBoth = new StreamWriter( "InBoth.txt", false );

            if( list != null ) {
                foreach( GameInfo g in list.Games.Values ) {
                    if( !appIds.Contains( g.Id ) ) {
                        writeInOldNotNew.WriteLine( string.Format("{0}: {1}", g.Id, g.Name ) );
                    } else {
                        writeInBoth.WriteLine( string.Format( "{0}: {1}", g.Id, g.Name ) );
                    }
                }
                foreach( uint aId in appIds ) {
                    if( !list.Games.ContainsKey((int)aId)) {
                        writeInNewNotOld.WriteLine( GetGameString( (int)aId, appList ) );
                    }
                }
            }

            writeInOldNotNew.Close();
            writeInNewNotOld.Close();
            writeInBoth.Close();
        }

        public static void TestNewReader() {
            Dictionary<int, AppInfo> appList = AppInfo.LoadApps( @"D:\Steam\appcache\appinfo.vdf" );
        }

        public static string GetGameString( int id, Dictionary<int, AppInfo> appList ) {
            string s = string.Format( "ID: {0} ", id );
            if( appList.ContainsKey( id ) ) {
                s += string.Format( " {0} ({1})", appList[id].name, appList[id].type );
            } else {
                s += "Not in Database";
            }
            return s;
        }
    }
}