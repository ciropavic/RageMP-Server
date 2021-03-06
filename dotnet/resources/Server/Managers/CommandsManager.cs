using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using GTANetworkAPI;
using Newtonsoft.Json;

namespace ServerSide
{
    public class CommandsManager
    {
        PlayerDataManager playerDataManager;
        VehicleDataManager vehicleDataManager = new VehicleDataManager();
        Houses houses;
        List<Report> reportsList = new List<Report>();
        public List<CarDealer> carDealers = new List<CarDealer>();
        OrgManager orgManager;
        List<TuneBusiness> testTune;
        LSPD lspd;
        PayoutManager payoutManager;
        public CommandsManager(Houses houses, List<CarDealer> carDealers, List<Report> reportsList, PlayerDataManager pdm, ref OrgManager orgManager, ref List<TuneBusiness> testTune, ref LSPD lspd, ref PayoutManager payoutManager)
        {
            this.houses = houses;
            this.carDealers = carDealers;
            this.reportsList = reportsList;
            this.playerDataManager = pdm;
            this.orgManager = orgManager;
            this.testTune = testTune;
            this.lspd = lspd;
            this.payoutManager = payoutManager;
        }

        public void ExecuteCommand(Player player, string command, string arguments)
        {
            List<string> args = new List<string>();
            string argText = "";
            if (arguments != null)
            {
                argText = arguments.TrimStart(' ');
            }
            if (arguments != null && arguments.Contains(' '))
            {
                args = new List<string>(arguments.TrimStart(' ').Split(' '));
            }
            else if (arguments != "" && arguments != null)
            {
                args.Add(arguments);
            }
            int argsCount = args.Count;
            int playersPower = player.GetSharedData<Int32>("power");

            //Gracz
            if (playersPower >= 0)
            {
                switch (command)
                {
                    case "reconnect":
                        player.KickSilent();
                        break;
                    case "o":
                        if (!player.GetSharedData<bool>("muted"))
                        {
                            if (argText.Replace(" ", "").Length > 0)
                            {
                                playerDataManager.SendMessageToOrg(player, argText);
                            }
                            else
                            {
                                playerDataManager.NotifyPlayer(player, "Wiadomo???? nie mo??e by?? pusta!");
                            }
                        }
                        else
                            playerDataManager.NotifyPlayer(player, "Jeste?? wyciszony do: " + player.GetSharedData<string>("mutedto"));
                        break;
                    case "g":
                        if (!player.GetSharedData<bool>("muted"))
                            if(argText.Replace(" ", "").Length > 0)
                            {
                                playerDataManager.SendGlobalMessage(player, argText);
                            }
                            else
                            {
                                playerDataManager.NotifyPlayer(player, "Wiadomo???? nie mo??e by?? pusta!");
                            }
                        else
                            playerDataManager.NotifyPlayer(player, "Jeste?? wyciszony do: " + player.GetSharedData<string>("mutedto"));
                        break;
                    case "pm":
                        if (!player.GetSharedData<bool>("muted"))
                        {
                            if (argsCount > 1)
                            {
                                string p = args[0];
                                string t = argText.Replace(p + " ", "");
                                if (t.Replace(" ", "").Length > 0)
                                {
                                    playerDataManager.SendPrivateMessage(player, p, t);
                                }
                                else
                                {
                                    playerDataManager.NotifyPlayer(player, "Wiadomo???? nie mo??e by?? pusta!");
                                }
                            }
                            else
                            {
                                playerDataManager.NotifyPlayer(player, "Nieprawid??owa sk??adnia komendy!");
                            }
                        }
                        else
                        {
                            playerDataManager.NotifyPlayer(player, "Jeste?? wyciszony do: " + player.GetSharedData<string>("mutedto"));
                        }
                        break;
                    case "report":
                        if (argsCount > 1)
                        {
                            string p = args[0];
                            string t = argText.Replace(p + " ", "");
                            if (t.Replace(" ", "").Length > 0)
                            {
                                Report report = playerDataManager.ReportAPlayer(player, p, t);
                                if (report == null)
                                {
                                    playerDataManager.NotifyPlayer(player, "Nie znaleziono gracza!");
                                }
                                else
                                {

                                    reportsList.Add(report);
                                    playerDataManager.NotifyAdmins();
                                    playerDataManager.SendInfoToPlayer(player, $"Raport o ID {report.id} zosta?? pomy??lnie wys??any!");
                                }
                            }
                            else
                            {
                                playerDataManager.NotifyPlayer(player, "Zg??oszenie nie mo??e by?? puste!");
                            }
                            
                        }
                        else
                        {
                            playerDataManager.NotifyPlayer(player, "Nieprawid??owa sk??adnia komendy!");
                        }
                        break;
                    case "kierowcy":
                        if(arguments == null)
                        {
                            if(player.Vehicle != null && player.Vehicle.Exists && player.Vehicle.HasSharedData("drivers") && player.VehicleSeat == 0)
                            {
                                playerDataManager.SendInfoToPlayer(player, $"Ostatni kierowcy {player.Vehicle.GetSharedData<Int32>("id")}: " + vehicleDataManager.GetVehiclesLastDrivers(player.Vehicle));
                            }
                            else
                            {
                                playerDataManager.NotifyPlayer(player, "Kierowc??w pojazdu mo??na sprawdzi?? tylko jako kierowca pojazdu osobistego!");
                            }
                        }
                        else
                        {
                            playerDataManager.NotifyPlayer(player, "Nieprawid??owa sk??adnia komendy!");
                        }
                        break;
                    case "bonus":
                        if(arguments == null)
                        {
                            playerDataManager.SendInfoToPlayer(player, $"Obecny bonus: {float.Parse(payoutManager.currentBonus[1]) * 100}% na {payoutManager.currentBonus[0]} do {payoutManager.bonusTime.ToShortTimeString()}");
                        }
                        else
                        {
                            playerDataManager.NotifyPlayer(player, "Nieprawid??owa sk??adnia komendy!");
                        }
                        break;
                    case "przelew":
                        if(args.Count == 1)
                        {
                            Player target = playerDataManager.GetPlayerByRemoteId(args[0]);
                            if(target != null && target != player)
                            {
                                if(target.Position.DistanceTo(player.Position) <= 10)
                                {
                                    player.TriggerEvent("openMoneyTransferBrowser", target);
                                }
                                else
                                {
                                    playerDataManager.NotifyPlayer(player, "Gracz jest za daleko!");
                                }
                                
                            }
                            else
                            {
                                playerDataManager.NotifyPlayer(player, "Nie znaleziono gracza!");
                            }
                        }
                        else
                        {
                            playerDataManager.NotifyPlayer(player, "Niepoprawna sk??adnia komendy!");
                        }
                        break;
                    case "time":
                        if(arguments == null)
                        {
                            playerDataManager.SendInfoToPlayer(player, DateTime.Now.ToString());
                        }
                        else
                        {
                            playerDataManager.NotifyPlayer(player, "Niepoprawna sk??adnia komendy!");
                        }
                        break;
                    case "areszt":
                        if(player.HasSharedData("lspd_duty") && player.GetSharedData<bool>("lspd_duty"))
                        {
                            int time;
                            if (args.Count == 1 && int.TryParse(args[0], out time))
                            {
                                if(time > 0)
                                {
                                    if (player.HasSharedData("cuffedPlayer") && player.GetSharedData<Player>("cuffedPlayer") != null && player.GetSharedData<Player>("cuffedPlayer").Exists)
                                    {
                                        Player cuffed = player.GetSharedData<Player>("cuffedPlayer");
                                        if (cuffed.Position.DistanceTo(lspd.ArrestPosition) <= 5)
                                        {
                                            cuffed.TriggerEvent("handCuff", player, false);
                                            cuffed.SetSharedData("handCuffed", false);
                                            player.ResetSharedData("cuffedPlayer");
                                            playerDataManager.NotifyPlayer(cuffed, "Zosta??e?? aresztowany na " + time.ToString() + " minut.");
                                            lspd.SetPlayerIntoArrest(cuffed, time);
                                        }
                                        else
                                        {
                                            playerDataManager.NotifyPlayer(player, "Gracza aresztowa?? mo??esz jedynie na komendzie LSPD");
                                        }
                                    }
                                    else
                                    {
                                        playerDataManager.NotifyPlayer(player, "Aby aresztowa?? gracza musisz go zaku??!");
                                    }
                                }
                                else
                                {
                                    playerDataManager.NotifyPlayer(player, "Czas musi by?? wi??kszy od 0!");
                                }
                            }
                            else
                            {
                                playerDataManager.NotifyPlayer(player, "Niepoprawna sk??adnia komendy!");
                            }
                        }
                        break;
                    case "pmoff":
                        if(args.Count > 0)
                        {
                            player.SetSharedData("pmoff", argText);
                            playerDataManager.NotifyPlayer(player, "Blokowanie wiadomo??ci prywatnych w????czone!");
                        }
                        else if(arguments == null)
                        {
                            player.SetSharedData("pmoff", "Brak.");
                            playerDataManager.NotifyPlayer(player, "Blokowanie wiadomo??ci prywatnych w????czone!");
                        }
                        break;
                    case "pmon":
                        if(arguments != null)
                        {
                            player.SetSharedData("pmoff", "");
                            playerDataManager.NotifyPlayer(player, "Wy????czono blokowanie wiadomo??ci prywatnych!");
                        }
                        break;
                }
            }

            //Tester
            if (playersPower >= 2)
            {
                switch(command)
                {
                    case "anim":
                        if (args.Count == 2)
                        {
                            player.TriggerEvent("testAnim", args[0], args[1]);
                        }
                        else
                        {
                            playerDataManager.NotifyPlayer(player, "Niepoprawna sk??adnia komendy!");
                        }
                        break;
                    case "anim2":
                        if (args.Count == 2)
                        {
                            player.TriggerEvent("testAnim2", args[0], args[1]);
                        }
                        else
                        {
                            playerDataManager.NotifyPlayer(player, "Niepoprawna sk??adnia komendy!");
                        }
                        break;
                    case "a":
                        if (!player.GetSharedData<bool>("muted"))
                            if (argText.Replace(" ", "").Length > 0)
                            {
                                playerDataManager.SendMessageToAdmins(player, argText);
                            }
                            else
                            {
                                playerDataManager.NotifyPlayer(player, "Wiadomo???? nie mo??e by?? pusta!");
                            }
                        else
                            playerDataManager.NotifyPlayer(player, "Jeste?? wyciszony do: " + player.GetSharedData<string>("mutedto"));
                        break;
                    case "fix":
                        if(argsCount == 1)
                        {
                            Vehicle veh = vehicleDataManager.GetVehicleById(args[0]);
                            if (veh != null)
                            {
                                vehicleDataManager.RepairVehicle(veh);
                            }
                            else
                            {
                                playerDataManager.NotifyPlayer(player, "Nie znaleziono pojazdu!");
                            }
                        }
                        break;
                    case "klucze":
                        if (argsCount == 2)
                        {
                            Vehicle veh = vehicleDataManager.GetVehicleById(args[0]);
                            if (veh != null && veh.Exists && (uint)veh.GetSharedData<Int64>("owner") == player.SocialClubId)
                            {
                                Player pl = playerDataManager.GetPlayerByRemoteId(args[1]);
                                if (pl != null && pl.Exists)
                                {
                                    if (pl.HasSharedData("carkeys") && veh.HasSharedData("id") && pl.GetSharedData<Int32>("carkeys") == veh.GetSharedData<Int32>("id"))
                                    {
                                        pl.SetSharedData("carkeys", -999999);
                                        playerDataManager.NotifyPlayer(pl, "Twoje klucze do pojazdu o ID: " + veh.GetSharedData<Int32>("id").ToString() + " zosta??y uniewa??nione!");
                                        playerDataManager.NotifyPlayer(player, "Odebra??e?? klucze do pojazdu o ID: " + veh.GetSharedData<Int32>("id").ToString() + " graczowi " + pl.GetSharedData<string>("username") + "!");
                                        if (pl.Vehicle == veh)
                                        {
                                            pl.WarpOutOfVehicle();
                                        }

                                    }
                                    else
                                    {
                                        pl.SetSharedData("carkeys", veh.GetSharedData<Int32>("id"));
                                        playerDataManager.NotifyPlayer(pl, "Otrzyma??e?? klucze do pojazdu o ID: " + veh.GetSharedData<Int32>("id").ToString() + "!");
                                        playerDataManager.NotifyPlayer(player, "Udost??pni??e?? klucze do  pojazdu o ID: " + veh.GetSharedData<Int32>("id").ToString() + " graczowi " + pl.GetSharedData<string>("username") + "!");
                                    }

                                }
                                else
                                {
                                    playerDataManager.NotifyPlayer(player, "Nie znaleziono gracza!");
                                }
                            }
                            else
                            {
                                playerDataManager.NotifyPlayer(player, "Nie znaleziono pojazdu!");
                            }

                        }
                        break;
                }
                
            }

            //Junior Moderator
            if (playersPower >= 3)
            {
                switch (command)
                {
                    //case "zakuj":
                    //    if (args.Count == 1)
                    //    {
                    //        Player p = playerDataManager.GetPlayerByRemoteId(args[0]);
                    //        if (p != null && (!p.HasSharedData("handCuffed") || p.HasSharedData("handCuffed") && !p.GetSharedData<bool>("handCuffed")) && p.Position.DistanceTo(player.Position) < 2)
                    //        {
                    //            p.TriggerEvent("handCuff", player, true);
                    //        }
                    //        else
                    //        {
                    //            playerDataManager.NotifyPlayer(player, "Nie odnaleziono gracza lub jest on za daleko!");
                    //        }
                    //    }
                    //    break;
                    //case "rozkuj":
                    //    if (args.Count == 1)
                    //    {
                    //        Player p = playerDataManager.GetPlayerByRemoteId(args[0]);
                    //        if (p != null && p.HasSharedData("handCuffed") && p.GetSharedData<bool>("handCuffed"))
                    //        {
                    //            if (player.HasSharedData("cuffedPlayer") && player.GetSharedData<Player>("cuffedPlayer") == p.Handle)
                    //            {
                    //                p.TriggerEvent("handCuff", player, false);
                    //                p.SetSharedData("handCuffed", false);
                    //                player.ResetSharedData("cuffedPlayer");
                    //            }
                    //            else
                    //            {
                    //                playerDataManager.NotifyPlayer(player, "Nie Ty zaku??e?? tego gracza!");
                    //            }

                    //        }
                    //        else
                    //        {
                    //            playerDataManager.NotifyPlayer(player, "Ten gracz nie jest zakuty!");
                    //        }
                    //    }
                    //    break;

                    case "duty":
                        if (arguments == null)
                        {
                            if (player.HasSharedData("lspd_duty") && player.GetSharedData<bool>("lspd_duty"))
                            {
                                player.SetSharedData("lspd_duty", false);
                                player.RemoveAllWeapons();
                                playerDataManager.LoadPlayersClothes(player);
                            }
                            else
                            {
                                player.SetSharedData("lspd_duty", true);
                                player.SetSharedData("lspd_power", 10);
                                player.GiveWeapon(WeaponHash.Stungun, 1000);
                                player.SetClothes(3, 0, 0);
                                player.SetClothes(4, 35, 0);
                                player.SetClothes(6, 25, 0);
                                player.SetClothes(8, 58, 0);
                                player.SetClothes(11, 55, 0);
                            }
                        }
                        break;

                    case "testbox":
                        if (arguments == null)
                        {
                            if (player.Vehicle != null)
                            {
                                player.TriggerEvent("testbox");
                            }
                        }
                        break;
                    case "cars":
                        player.Position = new Vector3(228.35027f, -984.62695f, -98.99994f);
                        player.TriggerEvent("showCars", vehicleDataManager.GetAllVehiclesModels());
                        break;
                    case "hash":
                        if (argsCount == 1)
                        {
                            try
                            {
                                if(Regex.IsMatch(args[0], "[a-zA-Z]"))
                                {
                                    if (player.Vehicle != null && player.Vehicle.HasSharedData("type") && player.Vehicle.GetSharedData<string>("type") == "furka")
                                    {
                                        player.Vehicle.Delete();
                                        Vehicle vfurka = NAPI.Vehicle.CreateVehicle(NAPI.Util.GetHashKey(args[0]), player.Position, player.Heading, 112, 112, numberPlate: "FURKA");
                                        vfurka.SetSharedData("type", "furka");
                                        player.SetIntoVehicle(vfurka.Handle, 0);
                                    }
                                    else if (player.Vehicle == null)
                                    {
                                        Vehicle vfurka = NAPI.Vehicle.CreateVehicle(NAPI.Util.GetHashKey(args[0]), player.Position, player.Heading, 112, 112, numberPlate: "FURKA");
                                        vfurka.SetSharedData("type", "furka");
                                        player.SetIntoVehicle(vfurka.Handle, 0);
                                    }
                                }
                                else
                                {
                                    uint dechash = Convert.ToUInt32(args[0]);
                                    if (player.Vehicle != null && player.Vehicle.HasSharedData("type") && player.Vehicle.GetSharedData<string>("type") == "furka")
                                    {
                                        player.Vehicle.Delete();
                                        Vehicle vfurka = NAPI.Vehicle.CreateVehicle(dechash, player.Position, player.Heading, 112, 112, numberPlate: "FURKA");
                                        vfurka.SetSharedData("type", "furka");
                                        player.SetIntoVehicle(vfurka.Handle, 0);
                                    }
                                    else if (player.Vehicle == null)
                                    {
                                        Vehicle vfurka = NAPI.Vehicle.CreateVehicle(dechash, player.Position, player.Heading, 112, 112, numberPlate: "FURKA");
                                        vfurka.SetSharedData("type", "furka");
                                        player.SetIntoVehicle(vfurka.Handle, 0);
                                    }
                                }
                                

                            }
                            catch (Exception)
                            {
                                playerDataManager.NotifyPlayer(player, "Co?? zjeba??e??");
                            }
                        }
                        break;
                    case "furka":
                        if (argsCount == 1)
                        {
                            if (player.Vehicle != null && player.Vehicle.HasSharedData("type") && player.Vehicle.GetSharedData<string>("type") == "furka")
                            {
                                player.Vehicle.Delete();
                                Vehicle vfurka = NAPI.Vehicle.CreateVehicle(NAPI.Util.VehicleNameToModel(args[0]), player.Position, player.Heading, 118, 118, numberPlate: "FURKA");
                                vfurka.Rotation = new Vector3(0,0,player.Heading);
                                vfurka.SetSharedData("type", "furka");
                                player.SetIntoVehicle(vfurka.Handle, 0);
                                vfurka.SetSharedData("collision", false);
                            }
                            else if (player.Vehicle == null)
                            {
                                Vehicle vfurka = NAPI.Vehicle.CreateVehicle(NAPI.Util.VehicleNameToModel(args[0]), player.Position, player.Heading, 118, 118, numberPlate: "FURKA");
                                vfurka.SetSharedData("type", "furka");
                                player.SetIntoVehicle(vfurka.Handle, 0);
                                vfurka.SetSharedData("collision", false);
                            }
                        }
                        else
                        {
                            playerDataManager.NotifyPlayer(player, "Nieprawid??owa sk??adnia komendy!");
                        }
                        break;
                    case "fdelete":
                        if (arguments == null)
                        {
                            foreach (Vehicle furkaV in NAPI.Pools.GetAllVehicles())
                            {
                                if (furkaV.HasSharedData("type") && furkaV.GetSharedData<string>("type") == "furka" && furkaV.Occupants.Count == 0)
                                {
                                    furkaV.Delete();
                                }
                            }
                        }
                        else
                            playerDataManager.NotifyPlayer(player, "Nieprawid??owa sk??adnia komendy!");
                        break;
                    case "kary":
                        if (argsCount == 1)
                        {
                            string name = args[0];
                            string social = playerDataManager.GetPlayersSocialByName(name);
                            if(social != "")
                            {
                                List<string[]> penalties = playerDataManager.GetPlayersPenalties(social, name);
                                if(penalties.Count > 0)
                                {
                                    player.TriggerEvent("openPenaltiesBrowser", JsonConvert.SerializeObject(penalties));
                                }
                                else
                                {
                                    playerDataManager.NotifyPlayer(player, "Gracz nie ma ??adnych kar!");
                                }
                            }
                            else
                            {
                                playerDataManager.NotifyPlayer(player, "Nie odnaleziono gracza!");
                            }
                        }
                        else
                        {
                            playerDataManager.NotifyPlayer(player, "Nieprawid??owa sk??adnia komendy!");
                        }
                        break;
                    case "vtpto":
                        if (argsCount == 1)
                        {
                            Vehicle veh = vehicleDataManager.GetVehicleById(args[0]);
                            if (veh != null)
                            {
                                if (player.Vehicle == null)
                                    player.Position = veh.Position;
                                else
                                    player.Vehicle.Position = veh.Position;
                            }
                            else
                            {
                                playerDataManager.NotifyPlayer(player, "Nie znaleziono pojazdu!");
                            }
                        }
                        else
                        {
                            playerDataManager.NotifyPlayer(player, "Nieprawid??owa sk??adnia komendy!");
                        }
                        break;
                    case "vtphere":
                        if (argsCount == 1)
                        {
                            Vehicle veh = vehicleDataManager.GetVehicleById(args[0]);
                            if (veh != null)
                            {
                                veh.SetSharedData("lastpos", player.Position);
                                veh.Position = player.Position;
                                if (veh.GetSharedData<bool>("veh_brake"))
                                {
                                    veh.SetSharedData("veh_brake", false);
                                    NAPI.Task.Run(() =>
                                    {
                                        veh.SetSharedData("veh_brake", true);
                                        vehicleDataManager.UpdateVehiclesLastPos(veh);
                                    }, 5000);
                                }
                                else
                                {
                                    vehicleDataManager.UpdateVehiclesLastPos(veh);
                                }
                            }
                            else
                            {
                                playerDataManager.NotifyPlayer(player, "Nie znaleziono pojazdu!");
                            }
                        }
                        else
                        {
                            playerDataManager.NotifyPlayer(player, "Nieprawid??owa sk??adnia komendy!");
                        }
                        break;

                    case "tpto":
                        if (argsCount == 1)
                        {
                            Player pl = playerDataManager.GetPlayerByRemoteId(args[0]);
                            if (pl != null && pl.Exists)
                            {
                                player.Position = pl.Position;
                            }
                            else
                            {
                                playerDataManager.NotifyPlayer(player, "Nie znaleziono gracza!");
                            }
                        }
                        else
                        {
                            playerDataManager.NotifyPlayer(player, "Nieprawid??owa sk??adnia komendy!");
                        }
                        break;

                    case "tphere":
                        if (argsCount == 1)
                        {
                            Player p = playerDataManager.GetPlayerByRemoteId(args[0]);
                            if (p != null)
                            {
                                if((p.HasSharedData("arrested") && p.GetSharedData<bool>("arrested")) || (p.HasSharedData("handCuffed") && p.GetSharedData<bool>("handCuffed")))
                                {
                                    playerDataManager.NotifyPlayer(player, "Nie mo??na teleportowa?? os??b aresztowanych");
                                }
                                else
                                {
                                    p.Position = player.Position;
                                }
                            }
                                
                            else
                            {
                                playerDataManager.NotifyPlayer(player, "Nie znaleziono gracza!");
                            }
                        }
                        else
                        {
                            playerDataManager.NotifyPlayer(player, "Nieprawid??owa sk??adnia komendy!");
                        }
                        break;

                    case "dp":
                        if (argsCount == 1)
                        {
                            Vehicle veh1 = vehicleDataManager.GetVehicleById(args[0]);
                            if (veh1 != null)
                            {
                                vehicleDataManager.UpdateVehicleSpawned(veh1, false);
                                playerDataManager.NotifyPlayer(player, $"Pojazd o ID: {args[0]} zosta?? przeniesiony do przechowalni!");
                                veh1.Delete();
                            }
                            else
                            {
                                playerDataManager.NotifyPlayer(player, "Nie znaleziono pojazdu!");
                            }
                        }
                        else
                        {
                            playerDataManager.NotifyPlayer(player, "Nieprawid??owa sk??adnia komendy!");
                        }
                        break;

                    case "zp":
                        if (argsCount == 1)
                        {
                            int id;
                            Vehicle veh2 = null;
                            if (Int32.TryParse(args[0], out id))
                            {
                                veh2 = vehicleDataManager.CreatePersonalVehicle(id, player.Position, player.Heading, false);
                            }

                            if (veh2 != null)
                            {
                                vehicleDataManager.UpdateVehicleSpawned(veh2, true);
                                vehicleDataManager.UpdateVehiclesLastPos(veh2);
                                orgManager.SetVehiclesOrg(veh2);
                                playerDataManager.NotifyPlayer(player, $"Pojazd o ID: {args[0]} wyci??gni??ty!");

                            }
                            else
                            {
                                playerDataManager.NotifyPlayer(player, "Nie znaleziono pojazdu!");
                            }
                        }
                        else
                        {
                            playerDataManager.NotifyPlayer(player, "Nieprawid??owa sk??adnia komendy!");
                        }
                        break;
                    case "spec":
                        if (argsCount == 1)
                        {
                            Player pl = playerDataManager.GetPlayerByRemoteId(args[0]);
                            if (pl != null)
                            {
                                player.Position = new Vector3(pl.Position.X, pl.Position.Y, pl.Position.Z + 10);
                                player.TriggerEvent("startSpectatingPlayer", pl);
                                player.Transparency = 0;
                                player.SetSharedData("spec", true);

                            }
                            else
                            {
                                playerDataManager.NotifyPlayer(player, "Nie znaleziono gracza!");
                            }
                        }
                        break;
                    case "stopspec":
                        if (arguments == null)
                        {
                            player.TriggerEvent("stopSpectatingPlayer");
                            player.Transparency = 255;
                            player.SetSharedData("spec", false);
                            player.Position = player.GetSharedData<Vector3>("lastpos");
                        }
                        break;
                    case "getowner":
                        if (argsCount == 1)
                        {
                            string vehowner = vehicleDataManager.GetVehiclesOwnerName(args[0]);
                            if (vehowner != "")
                            {
                                playerDataManager.SendInfoToPlayer(player, "Pojazd nale??y do " + vehowner);
                            }
                            else
                            {
                                playerDataManager.NotifyPlayer(player, "Nie odnaleziono pojazdu!");
                            }
                        }
                        break;
                    case "tpwp":
                        if (arguments == null)
                            player.TriggerEvent("teleportToWaypoint");
                        else
                            playerDataManager.NotifyPlayer(player, "Nieprawid??owa sk??adnia komendy!");
                        break;
                    case "warn":
                        if (argsCount >= 2)
                        {
                            Player p = playerDataManager.GetPlayerByRemoteId(args[0]);
                            if (p != null)
                            {
                                string reason = argText.Replace(args[0], "").TrimStart(' ');
                                playerDataManager.warnPlayer(p, reason, player);
                            }
                            else
                            {
                                playerDataManager.NotifyPlayer(player, "Nie znaleziono gracza!");
                            }
                        }
                        else
                        {
                            playerDataManager.NotifyPlayer(player, "Nieprawid??owa sk??adnia komendy!");
                        }
                        break;
                    case "kick":
                        if (argsCount >= 2)
                        {
                            Player p = playerDataManager.GetPlayerByRemoteId(args[0]);
                            if (p != null)
                            {
                                string reason = argText.Replace(args[0], "").TrimStart(' ');
                                playerDataManager.kickPlayer(p, reason, player);
                            }
                            else
                            {
                                playerDataManager.NotifyPlayer(player, "Nie znaleziono gracza!");
                            }
                        }
                        else
                        {
                            playerDataManager.NotifyPlayer(player, "Nieprawid??owa sk??adnia komendy!");
                        }
                        break;
                    case "getpos":
                        if (argsCount > 0)
                        {
                            string position = $"new Vector3({player.Position.X.ToString().Replace(',', '.')}f, {player.Position.Y.ToString().Replace(',', '.')}f, {player.Position.Z.ToString().Replace(',', '.')}f); heading: {player.Heading.ToString().Replace(',', '.')}f  -  {argText}";
                            File.AppendAllText(@"positions.txt", position + Environment.NewLine, new UTF8Encoding(false, true));
                            playerDataManager.NotifyPlayer(player, "Pozycja zapisana w pliku!");
                        }
                        else
                        {
                            playerDataManager.NotifyPlayer(player, "Nieprawid??owa sk??adnia komendy!");
                        }
                        break;

                    case "note":
                        if (argsCount > 0)
                        {
                            File.AppendAllText(@"notes.txt", player.GetSharedData<string>("username") + ": " + argText + Environment.NewLine, new UTF8Encoding(false, true));
                            playerDataManager.NotifyPlayer(player, "Notatka zapisana w pliku!");
                        }
                        else
                        {
                            playerDataManager.NotifyPlayer(player, "Nieprawid??owa sk??adnia komendy!");
                        }
                        break;
                }
            }

            //Moderator
            if (playersPower >= 4)
            {
                switch (command)
                {
                    case "coflic":
                        if (argsCount >= 2)
                        {
                            Player p = playerDataManager.GetPlayerByRemoteId(args[0]);
                            if (p != null)
                            {
                                string reason = argText.Replace(args[0], "").TrimStart(' ');
                                playerDataManager.unLicence(p, reason, player);
                            }
                            else
                            {
                                playerDataManager.NotifyPlayer(player, "Nie znaleziono gracza!");
                            }
                        }
                        else
                        {
                            playerDataManager.NotifyPlayer(player, "Nieprawid??owa sk??adnia komendy!");
                        }
                        break;


                    case "licence":
                        if (argsCount >= 3)
                        {
                            Player p = playerDataManager.GetPlayerByRemoteId(args[0]);
                            if (p != null)
                            {
                                string reason = argText.Replace(args[0], "").Replace(args[1], "").TrimStart(' ');
                                playerDataManager.takeLicence(p, args[1], reason, player);
                            }
                            else
                            {
                                playerDataManager.NotifyPlayer(player, "Nie znaleziono gracza!");
                            }
                        }
                        else
                        {
                            playerDataManager.NotifyPlayer(player, "Nieprawid??owa sk??adnia komendy!");
                        }
                        break;

                    case "mute":
                        if (argsCount >= 3)
                        {
                            Player p = playerDataManager.GetPlayerByRemoteId(args[0]);
                            if (p != null)
                            {
                                string reason = argText.Replace(args[0], "").Replace(args[1], "").TrimStart(' ');
                                playerDataManager.mutePlayer(p, args[1], reason, player);
                            }
                            else
                            {
                                playerDataManager.NotifyPlayer(player, "Nie znaleziono gracza!");
                            }
                        }
                        else
                        {
                            playerDataManager.NotifyPlayer(player, "Nieprawid??owa sk??adnia komendy!");
                        }
                        break;
                }
            }

            //SuperModerator
            if (playersPower >= 5)
            {
                switch (command)
                {
                    case "ban":
                        if (argsCount >= 3)
                        {
                            Player p = playerDataManager.GetPlayerByRemoteId(args[0]);
                            if (p != null)
                            {
                                string reason = argText.Replace(args[0], "").Replace(args[1], "").TrimStart(' ');
                                playerDataManager.banPlayer(p, args[1], reason, player);
                            }
                            else
                            {
                                playerDataManager.NotifyPlayer(player, "Nie znaleziono gracza!");
                            }
                        }
                        else
                        {
                            playerDataManager.NotifyPlayer(player, "Nieprawid??owa sk??adnia komendy!");
                        }
                        break;

                    case "tp":
                        if (argsCount == 3)
                        {
                            float x = 0, y = 0, z = 0;
                            if (float.TryParse(args[0], out x) && float.TryParse(args[1], out y) && float.TryParse(args[2], out z))
                            {
                                Vector3 posv = new Vector3(x, y, z);
                                player.Position = posv;
                            }
                            else
                            {
                                playerDataManager.NotifyPlayer(player, "Podano b????dne warto??ci!");
                            }
                        }
                        else
                        {
                            playerDataManager.NotifyPlayer(player, "Nieprawid??owa sk??adnia komendy!");
                        }
                        break;
                    case "kys":
                        if (arguments == null)
                        {
                            player.Kill();
                        }
                        else
                        {
                            playerDataManager.NotifyPlayer(player, "Nieprawid??owa sk??adnia komendy!");
                        }
                        break;
                }
            }

            //Junior Admin
            if (playersPower >= 6)
            {
                switch (command)
                {
                    case "setday":
                        if (arguments == null)
                        {
                            playerDataManager.time = new DateTime(2000, 12, 12, 12, 0, 0);
                        }
                        else
                        {
                            playerDataManager.NotifyPlayer(player, "Nieprawid??owa sk??adnia komendy!");
                        }
                        break;

                    case "setnight":
                        if (arguments == null)
                        {
                            playerDataManager.time = new DateTime(2000, 12, 12, 0, 0, 0);
                        }
                        else
                        {
                            playerDataManager.NotifyPlayer(player, "Nieprawid??owa sk??adnia komendy!");
                        }
                        break;
                }
            }

            //Admin
            if (playersPower >= 7)
            {
                // switch (command)
                // {
                //     case "getpos":
                //         if (argsCount > 0)
                //         {
                //             string position = $"new Vector3({player.Position.X.ToString().Replace(',', '.')}f, {player.Position.Y.ToString().Replace(',', '.')}f, {player.Position.Z.ToString().Replace(',', '.')}f); heading: {player.Heading.ToString().Replace(',', '.')}f  -  {argText}";
                //             File.AppendAllText(@"positions.txt", position + Environment.NewLine, new UTF8Encoding(false, true));
                //             playerDataManager.NotifyPlayer(player, "Pozycja zapisana w pliku!");
                //         }
                //         else
                //         {
                //             playerDataManager.NotifyPlayer(player, "Nieprawid??owa sk??adnia komendy!");
                //         }
                //         break;

                //     case "note":
                //         if (argsCount > 0)
                //         {
                //             File.AppendAllText(@"notes.txt", player.GetSharedData<string>("username") + ": " + argText + Environment.NewLine, new UTF8Encoding(false, true));
                //             playerDataManager.NotifyPlayer(player, "Notatka zapisana w pliku!");
                //         }
                //         else
                //         {
                //             playerDataManager.NotifyPlayer(player, "Nieprawid??owa sk??adnia komendy!");
                //         }
                //         break;
                // }
            }

            //Owner
            if (playersPower >= 10)
            {
                switch (command)
                {
                    case "inv":
                        player.TriggerEvent("setPlayerInvincible");
                        break;
                    case "createhouse":
                        if (argsCount == 2)
                        {
                            try
                            {
                                string type = args[0];
                                int price = Convert.ToInt32(args[1]);
                                Vector3 pos = player.Position;
                                houses.AddHouse(pos, type, price);
                            }
                            catch (Exception)
                            {
                                playerDataManager.NotifyPlayer(player, "Poda??e?? nieprawid??ow?? cen??!");
                            }
                        }
                        else
                        {
                            playerDataManager.NotifyPlayer(player, "Nieprawid??owa sk??adnia komendy!");
                        }
                        break;
                    case "komis":
                        CarDealer closest = null;
                        foreach (CarDealer cd in carDealers)
                        {
                            if (closest == null && cd.position.DistanceTo(player.Position) < 8.0f)
                            {
                                closest = cd;
                            }
                            else if (closest != null && cd.position.DistanceTo(player.Position) < closest.position.DistanceTo(player.Position))
                            {
                                closest = cd;
                            }
                        }
                        if (closest != null)
                        {
                            closest.SpawnNew(true, true);
                        }
                        break;
                    case "additem":
                        int itemId;
                        if (argsCount == 1 && Int32.TryParse(args[0], out itemId))
                        {
                            player.TriggerEvent("addItemToEquipment", itemId);
                        }
                        else
                        {
                            playerDataManager.NotifyPlayer(player, "Nieprawid??owa sk??adnia komendy!");
                        }
                        break;
                    case "ray":
                        player.TriggerEvent("rayCastTest");
                        break;
                    case "sound":
                        if (argsCount == 1)
                        {
                            Player pla = playerDataManager.GetPlayerByRemoteId(args[0]);
                            if (pla != null && pla.Exists)
                            {
                                pla.TriggerEvent("playmusic");
                            }
                            else
                            {
                                playerDataManager.NotifyPlayer(player, "Nie znaleziono gracza!");
                            }
                        }
                        else
                        {
                            playerDataManager.NotifyPlayer(player, "Nieprawid??owa sk??adnia komendy!");
                        }
                        break;
                    case "xmas":
                        if(arguments == null){
                            NAPI.World.SetWeather(Weather.XMAS);
                        }
                        break;
                    case "clear":
                        if(arguments == null){
                            NAPI.World.SetWeather(Weather.EXTRASUNNY);
                        }
                        break;
                    case "power":
                        if(argsCount == 1)
                        {
                            float val = 0;
                            if(float.TryParse(args[0], out val))
                            {
                                if(player.Vehicle != null && player.Vehicle.HasSharedData("type") && player.Vehicle.GetSharedData<string>("type") == "furka")
                                {
                                    player.TriggerEvent("setHandlings", "power", val);
                                }
                                else
                                {
                                    playerDataManager.NotifyPlayer(player, "Tylko w furce!");
                                }
                            }
                            else
                            {
                                playerDataManager.NotifyPlayer(player, "Nieprawid??owe warto??ci!");
                            }
                        }
                        break;
                    case "speed":
                        if (argsCount == 1)
                        {
                            float val = 0;
                            if (float.TryParse(args[0], out val))
                            {
                                if (player.Vehicle != null && player.Vehicle.HasSharedData("type") && player.Vehicle.GetSharedData<string>("type") == "furka")
                                {
                                    player.TriggerEvent("setHandlings", "speed", val);
                                }
                                else
                                {
                                    playerDataManager.NotifyPlayer(player, "Tylko w furce!");
                                }
                            }
                            else
                            {
                                playerDataManager.NotifyPlayer(player, "Nieprawid??owe warto??ci!");
                            }
                        }
                        break;
                    case "save":
                        if(arguments == null)
                        {
                            player.TriggerEvent("saveVehiclesSettings");
                        }
                        break;
                    case "zds":
                        if(argsCount == 1)
                        {
                            int speed = 0;
                            if(Int32.TryParse(args[0], out speed))
                            {
                                player.TriggerEvent("zts", speed);
                            }
                        }
                        break;
                    case "felgi":
                        if(argsCount == 3 && player.Vehicle != null)
                        {
                            int a, b, c;
                            if(int.TryParse(args[0], out a) && int.TryParse(args[1], out b) && int.TryParse(args[2], out c))
                            {
                                NAPI.Vehicle.SetVehicleWheelType(player.Vehicle.Handle, b);
                                player.Vehicle.SetMod(23, a);
                                NAPI.Vehicle.SetVehicleCustomTires(player.Vehicle.Handle, c == 1 ? true : false);
                                
                            }
                        }
                        break;
                    case "bomba":
                        if (argsCount == 2)
                        {
                            Player play = playerDataManager.GetPlayerByRemoteId(args[0]);
                            if (play != null && play.Exists)
                            {
                                play.TriggerEvent("openTrollBrowser", args[1]);
                            }
                            else
                            {
                                playerDataManager.NotifyPlayer(player, "Nie znaleziono gracza!");
                            }
                        }
                        else
                        {
                            playerDataManager.NotifyPlayer(player, "Nieprawid??owa sk??adnia komendy!");
                        }
                        break;
                    case "showbones":
                        if(arguments == null)
                        {
                            if(player.Vehicle != null)
                            {
                                player.TriggerEvent("showBones");
                            }
                        }
                        break;
                    case "barrier":
                        if (arguments == null)
                        {
                            if (player.Vehicle == null)
                            {
                                player.TriggerEvent("createBarrier");
                            }
                        }
                        break;
                    case "dirt":
                        if (args.Count == 1)
                        {
                            if (player.Vehicle != null)
                            {
                                player.TriggerEvent("test_setDirtStick", args[0]);
                            }
                        }
                        break;
                    case "drag":
                        if (args.Count == 2)
                        {
                            if (player.Vehicle != null)
                            {
                                player.TriggerEvent("test_setDrag", args[0], args[1]);
                            }
                        }
                        break;
                    case "w":
                        if(args.Count == 1)
                        {
                            if(player.Vehicle != null)
                            {
                                for(int i = 0; i < 20; i++)
                                {
                                    player.Vehicle.SetExtra(i, false);
                                }
                                int d;
                                if(int.TryParse(args[0], out d))
                                {
                                    player.Vehicle.SetExtra(d, true);
                                }
                            }
                        }
                        break;
                    case "newbonus":
                        if(arguments == null)
                        {
                            string[] bonus = payoutManager.SetNewBonus();
                            playerDataManager.SendInfoMessageToAllPlayers($"Wylosowano nowy bonus {float.Parse(bonus[1]) * 100}% na: {bonus[0]} do: {payoutManager.bonusTime}");
                        }
                        break;
                    case "logcar":
                        CarDealer closest2 = null;
                        foreach (CarDealer cd in carDealers)
                        {
                            if (closest2 == null && cd.position.DistanceTo(player.Position) < 8.0f)
                            {
                                closest2 = cd;
                            }
                            else if (closest2 != null && cd.position.DistanceTo(player.Position) < closest2.position.DistanceTo(player.Position))
                            {
                                closest2 = cd;
                            }
                        }
                        if (closest2 != null && closest2.vehicle.Exists)
                        {
                            playerDataManager.SendInfoToPlayer(player, JsonConvert.SerializeObject(closest2.vehicle));
                        }
                        break;
                    case "lspdadd":
                        Player pl = playerDataManager.GetPlayerByRemoteId(args[0]);
                        if (pl != null && pl.Exists)
                        {
                            if(lspd.AddPlayerToGroup(pl))
                            {
                                playerDataManager.NotifyPlayer(player, "Pomy??lnie dodano gracza do frakcji!");
                            }
                            else
                            {
                                playerDataManager.NotifyPlayer(player, "Dodanie gracza nie powiod??o si??!");
                            }
                        }
                        else
                        {
                            playerDataManager.NotifyPlayer(player, "Nie znaleziono gracza!");
                        }
                        break;
                    case "lspdremove":
                        Player p = playerDataManager.GetPlayerByRemoteId(args[0]);
                        if (p != null && p.Exists)
                        {
                            if (lspd.RemovePlayerFromGroup(p))
                            {
                                playerDataManager.NotifyPlayer(player, "Pomy??lnie usuni??to gracza do frakcji!");
                            }
                            else
                            {
                                playerDataManager.NotifyPlayer(player, "Usuni??cie gracza nie powiod??o si??!");
                            }
                        }
                        else
                        {
                            playerDataManager.NotifyPlayer(player, "Nie znaleziono gracza!");
                        }
                        break;
                    case "brama":
                        if(args.Count == 1)
                        {
                            int a;
                            if(int.TryParse(args[0], out a))
                            {
                                lspd.SwitchMainGate(a == 0 ? false : true);
                            }
                        }
                        break;
                    case "getmod":
                        if(args.Count == 1 && player.Vehicle != null)
                        {
                            int id;
                            if (int.TryParse(args[0], out id))
                                playerDataManager.SendInfoToPlayer(player, NAPI.Vehicle.GetVehicleMod(player.Vehicle.Handle, id).ToString());
                        }
                        break;
                    case "setmod":
                        if (args.Count == 2 && player.Vehicle != null)
                        {
                            int id, id2;
                            if (int.TryParse(args[0], out id) && int.TryParse(args[1], out id2))
                                NAPI.Vehicle.SetVehicleMod(player.Vehicle.Handle, id, id2);
                        }
                        break;
                    case "obj":
                        if (args.Count == 1)
                        {
                            player.TriggerEvent("test_createObject", args[0]);
                        }
                        break;
                    case "maxpoints":
                        if(args.Count == 1)
                        {
                            Player pointsPl = playerDataManager.GetPlayerByRemoteId(args[0]);
                            if (pointsPl != null && pointsPl.Exists)
                            {
                                playerDataManager.MaxPlayersJobPoints(pointsPl);
                                playerDataManager.NotifyPlayer(player, "Pomy??lnie zwi??kszono punkty pracy gracza do 2000!");
                            }
                            else
                            {
                                playerDataManager.NotifyPlayer(player, "Nie znaleziono gracza!");
                            }
                        }
                        break;
                }
            }
        }

        public void ExecuteConsoleCommand(string cmd)
        {
            string command;
            string arguments;
            string text = cmd.Substring(1, cmd.Length-1);
            if (text.Contains(' '))
            {
                command = text.Split(" ")[0];
                arguments = text.Replace(command + " ", "");
            }
            else
            {
                command = text;
                arguments = null;
            }

            List<string> args = new List<string>();
            string argText = "";
            if (arguments != null)
            {
                argText = arguments.TrimStart(' ');
            }
            if (arguments != null && arguments.Contains(' '))
            {
                args = new List<string>(arguments.TrimStart(' ').Split(' '));
            }
            else if (arguments != "" && arguments != null)
            {
                args.Add(arguments);
            }
            int argsCount = args.Count;


            switch(command)
            {
                case "warn":
                    if (argsCount >= 2)
                    {
                        Player p = playerDataManager.GetPlayerByRemoteId(args[0]);
                        if (p != null)
                        {
                            p.TriggerEvent("warnSound");
                            p.TriggerEvent("warnPlayer");
                            string reason = argText.Replace(args[0], "").TrimStart(' ');
                            playerDataManager.SendPenaltyToPlayers(p.GetSharedData<string>("username") + " otrzyma?? ostrze??enie od CONSOLE. Pow??d: " + reason);
                            playerDataManager.AddPenaltyToDB(p.SocialClubId.ToString(), "CONSOLE", "warn", DateTime.Now.ToString(), "", reason);
                        }
                        else
                        {
                            playerDataManager.SendInfoToConsole("Nie znaleziono gracza!");
                        }
                    }
                    else
                    {
                        playerDataManager.SendInfoToConsole("Nieprawid??owa sk??adnia komendy!");
                    }
                    break;

                case "kick":
                    if (argsCount >= 2)
                    {
                        Player p = playerDataManager.GetPlayerByRemoteId(args[0]);
                        if (p != null)
                        {
                            string reason = argText.Replace(args[0], "").TrimStart(' ');
                            p.Kick(reason);
                            playerDataManager.SendPenaltyToPlayers(p.GetSharedData<string>("username") + " zosta?? wyrzucony przez " + "CONSOLE" + ". Pow??d: " + reason);
                            playerDataManager.AddPenaltyToDB(p.SocialClubId.ToString(), "CONSOLE", "kick", DateTime.Now.ToString(), "", reason);
                        }
                        else
                        {
                            playerDataManager.SendInfoToConsole("Nie znaleziono gracza!");
                        }
                    }
                    else
                    {
                        playerDataManager.SendInfoToConsole("Nieprawid??owa sk??adnia komendy!");
                    }
                    break;
                case "ban":
                    if (argsCount >= 3)
                    {
                        Player p = playerDataManager.GetPlayerByRemoteId(args[0]);
                        if (p != null)
                        {
                            string reason = argText.Replace(args[0], "").Replace(args[1], "").TrimStart(' ');
                            playerDataManager.banPlayer(p, args[1], reason, null);
                        }
                        else
                        {
                            playerDataManager.SendInfoToConsole("Nie znaleziono gracza!");
                        }
                    }
                    else
                    {
                        playerDataManager.SendInfoToConsole("Nieprawid??owa sk??adnia komendy!");
                    }
                    break;
                case "dp":
                    if (argsCount == 1)
                    {
                        Vehicle veh1 = vehicleDataManager.GetVehicleById(args[0]);
                        if (veh1 != null)
                        {
                            vehicleDataManager.UpdateVehicleSpawned(veh1, false);
                            vehicleDataManager.UpdateVehiclesDamage(veh1, vehicleDataManager.wreckedDamage);
                            playerDataManager.SendInfoToConsole($"Pojazd o ID: {args[0]} zosta?? przeniesiony do przechowalni!");
                            NAPI.Task.Run(() => { veh1.Delete(); });
                        }
                        else
                        {
                            playerDataManager.SendInfoToConsole("Nie znaleziono pojazdu!");
                        }
                    }
                    else
                    {
                        playerDataManager.SendInfoToConsole("Nieprawid??owa sk??adnia komendy!");
                    }
                    break;
                case "maxpoints":
                    if (args.Count == 1)
                    {
                        Player pointsPl = playerDataManager.GetPlayerByRemoteId(args[0]);
                        if (pointsPl != null && pointsPl.Exists)
                        {
                            playerDataManager.MaxPlayersJobPoints(pointsPl);
                            playerDataManager.SendInfoToConsole("Pomy??lnie zwi??kszono punkty pracy gracza do 2000!");
                        }
                        else
                        {
                            playerDataManager.SendInfoToConsole("Nie znaleziono gracza!");
                        }
                    }
                    break;
            }
        }
    }
}
