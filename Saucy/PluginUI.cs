using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Components;
using Dalamud.Utility.Signatures;
using ECommons;
using ECommons.DalamudServices;
using ECommons.ImGuiMethods;
using FFTriadBuddy;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ImGuiNET;
using PunishLib.ImGuiMethods;
using Saucy.CuffACur;
using Saucy.OtherGames;
using Saucy.TripleTriad;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using TriadBuddyPlugin;
using static System.Windows.Forms.AxHost;


namespace Saucy
{
    // It is good to have this be disposable in general, in case you ever need it
    // to do any cleanup
    public unsafe class PluginUI : IDisposable
    {
        private Configuration configuration;
        
        // this extra bool exists for ImGui, since you can't ref a property
        private bool visible = false;
        public bool Visible
        {
            get { return visible; }
            set { visible = value; }
        }

        private bool settingsVisible = false;
        private GameNpcInfo currentNPC;

        public bool SettingsVisible
        {
            get { return settingsVisible; }
            set { settingsVisible = value; }
        }

        public GameNpcInfo CurrentNPC
        {
            get => currentNPC;
            set
            {
                if (currentNPC != value)
                {
                    TriadAutomater.TempCardsWonList.Clear();
                    currentNPC = value;
                }
            }
        }

        public PluginUI(Configuration configuration)
        {
            this.configuration = Saucy.Config;
        }

        public void Dispose()
        {
        }

        public bool Enabled { get; set; } = false;

        public void Draw()
        {
            DrawMainWindow();
        }

        public void DrawMainWindow()
        {
            if (!Visible)
            {
                return;
            }

            ImGui.SetNextWindowSize(new Vector2(520, 420), ImGuiCond.FirstUseEver);
            //ImGui.SetNextWindowSizeConstraints(new Vector2(520, 420), new Vector2(float.MaxValue, float.MaxValue));
            if (ImGui.Begin("Saucy 配置", ref visible))
            {
                if (ImGui.BeginTabBar("###游戏", ImGuiTabBarFlags.Reorderable))
                {
                    if (ImGui.BeginTabItem("重击伽美蛇"))
                    {
                        DrawCufTab();
                        ImGui.EndTabItem();
                    }

                    if (Saucy.openTT)
                    {
                        Saucy.openTT = false;
                        if (ImGuiEx.BeginTabItem("九宫幻卡", ImGuiTabItemFlags.SetSelected))
                        {
                            DrawTriadTab();
                            ImGui.EndTabItem();
                        }
                    }
                    else
                    {
                        if (ImGui.BeginTabItem("九宫幻卡"))
                        {
                            DrawTriadTab();
                            ImGui.EndTabItem();
                        }
                    }

                    if (ImGui.BeginTabItem("其他游戏"))
                    {
                        DrawOtherGamesTab();
                        ImGui.EndTabItem();
                    }


                    if (ImGui.BeginTabItem("统计数据"))
                    {
                        DrawStatsTab();
                        ImGui.EndTabItem();
                    }

                    if (ImGui.BeginTabItem("关于"))
                    {
                        AboutTab.Draw("Saucy");
                        ImGui.EndTabItem();
                    }

                    ImGui.EndTabBar();
                }
            }
            ImGui.End();
        }


        private void DrawOtherGamesTab()
        {
            //ImGui.Checkbox("启用 Air Force One 模块", ref AirForceOneModule.ModuleEnabled);

            var sliceIsRightEnabled = SliceIsRightModule.ModuleEnabled;
            if (ImGui.Checkbox("启用重击伽美蛇模块", ref sliceIsRightEnabled))
            {
                SliceIsRightModule.ModuleEnabled = sliceIsRightEnabled;
                Saucy.Config.Save();
            }
        }

        private void DrawStatsTab()
        {
            if (ImGui.BeginTabBar("统计数据"))
            {
                ImGui.Columns(3, "统计", false);
                ImGui.NextColumn();
                ImGuiEx.CenterColumnText(ImGuiColors.ParsedGold, "SAUCY 统计数据", true);
                ImGui.Columns(1);

                if (ImGui.BeginTabItem("所有"))
                {
                    this.DrawStatsTab(Saucy.Config.Stats, out bool reset);

                    if (reset)
                    {
                        Saucy.Config.Stats = new();
                        Saucy.Config.Save();
                    }

                    ImGui.EndTabItem();
                }
                if (ImGui.BeginTabItem("会话"))
                {
                    this.DrawStatsTab(Saucy.Config.SessionStats, out bool reset);
                    if (reset)
                        Saucy.Config.SessionStats = new();
                    ImGui.EndTabItem();
                }

                ImGui.EndTabBar();
            }
        }


        private void DrawStatsTab(Stats stat, out bool reset)
        {
            if (ImGui.BeginTabBar("游戏"))
            {
                if (ImGui.BeginTabItem("重击伽美蛇"))
                {
                    DrawCuffStats(stat);
                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("九宫幻卡"))
                {
                    DrawTTStats(stat);
                    ImGui.EndTabItem();
                }

                ImGui.EndTabBar();
            }

            ImGui.PushItemWidth(ImGui.GetContentRegionAvail().X);
            reset = ImGui.Button("重置统计数据（按住 Ctrl）", new Vector2(ImGui.GetContentRegionAvail().X, ImGui.GetContentRegionAvail().Y)) && ImGui.GetIO().KeyCtrl;
        }

        private void DrawCuffStats(Stats stat)
        {
            ImGui.BeginChild("Cuff 统计", new Vector2(0, ImGui.GetContentRegionAvail().Y - 30f), true);
            ImGui.Columns(3, null, false);
            ImGui.NextColumn();
            ImGuiEx.CenterColumnText(ImGuiColors.DalamudRed, "Cuff-a-cur", true);
            ImGuiHelpers.ScaledDummy(10f);
            ImGui.NextColumn();
            ImGui.NextColumn();
            ImGui.NextColumn();
            ImGuiEx.CenterColumnText("游戏次数", true);
            ImGui.NextColumn();
            ImGui.NextColumn();
            ImGui.NextColumn();
            ImGuiEx.CenterColumnText($"{stat.CuffGamesPlayed.ToString("N0")}");
            ImGui.NextColumn();
            ImGui.NextColumn();
            ImGui.Spacing();
            ImGuiEx.CenterColumnText("激烈！", true);
            ImGui.NextColumn();
            ImGuiEx.CenterColumnText("猛烈！！", true);
            ImGui.NextColumn();
            ImGuiEx.CenterColumnText("残酷！！！", true);
            ImGui.NextColumn();
            ImGuiEx.CenterColumnText($"{stat.CuffBruisings.ToString("N0")}");
            ImGui.NextColumn();
            ImGuiEx.CenterColumnText($"{stat.CuffPunishings.ToString("N0")}");
            ImGui.NextColumn();
            ImGuiEx.CenterColumnText($"{stat.CuffBrutals.ToString("N0")}");
            ImGui.NextColumn();
            ImGui.NextColumn();
            ImGuiEx.CenterColumnText("赢得MGP", true);
            ImGui.NextColumn();
            ImGui.NextColumn();
            ImGui.NextColumn();
            ImGuiEx.CenterColumnText($"{stat.CuffMGP.ToString("N0")}");

            ImGui.EndChild();
        }


        private void DrawTTStats(Stats stat)
        {
            ImGui.BeginChild("TT 统计", new Vector2(0, ImGui.GetContentRegionAvail().Y - 30f), true);
            ImGui.Columns(3, null, false);
            ImGui.NextColumn();
            ImGuiEx.CenterColumnText(ImGuiColors.DalamudRed, "九宫幻卡", true);
            ImGuiHelpers.ScaledDummy(10f);
            ImGui.NextColumn();
            ImGui.NextColumn();
            ImGui.NextColumn();
            ImGuiEx.CenterColumnText("游戏次数", true);
            ImGui.NextColumn();
            ImGui.NextColumn();
            ImGui.NextColumn();
            ImGuiEx.CenterColumnText($"{stat.GamesPlayedWithSaucy.ToString("N0")}");
            ImGui.NextColumn();
            ImGui.NextColumn();
            ImGui.Spacing();
            ImGuiEx.CenterColumnText("胜利", true);
            ImGui.NextColumn();
            ImGuiEx.CenterColumnText("失败", true);
            ImGui.NextColumn();
            ImGuiEx.CenterColumnText("平局", true);
            ImGui.NextColumn();
            ImGuiEx.CenterColumnText($"{stat.GamesWonWithSaucy.ToString("N0")}");
            ImGui.NextColumn();
            ImGuiEx.CenterColumnText($"{stat.GamesLostWithSaucy.ToString("N0")}");
            ImGui.NextColumn();
            ImGuiEx.CenterColumnText($"{stat.GamesDrawnWithSaucy.ToString("N0")}");
            ImGui.NextColumn();
            ImGuiEx.CenterColumnText("胜率", true);
            ImGui.NextColumn();
            ImGuiEx.CenterColumnText("获得卡片", true);
            ImGui.NextColumn();
            if (stat.NPCsPlayed.Count > 0)
            {
                ImGuiEx.CenterColumnText("对战最多的NPC", true);
                ImGui.NextColumn();
            }
            else
            {
                ImGui.NextColumn();
            }

            if (stat.GamesPlayedWithSaucy > 0)
            {
                ImGuiEx.CenterColumnText($"{Math.Round(((double)stat.GamesWonWithSaucy / (double)stat.GamesPlayedWithSaucy) * 100, 2)}%");
            }
            else
            {
                ImGuiEx.CenterColumnText("");
            }
            ImGui.NextColumn();
            ImGuiEx.CenterColumnText($"{stat.CardsDroppedWithSaucy.ToString("N0")}");
            ImGui.NextColumn();

            if (stat.NPCsPlayed.Count > 0)
            {
                ImGuiEx.CenterColumnText($"{stat.NPCsPlayed.OrderByDescending(x => x.Value).First().Key}");
                ImGuiEx.CenterColumnText($"{stat.NPCsPlayed.OrderByDescending(x => x.Value).First().Value.ToString("N0")} 次");
                ImGui.NextColumn();
                ImGui.NextColumn();
                ImGui.NextColumn();
            }

            ImGui.NextColumn();
            ImGuiEx.CenterColumnText("赢得MGP", true);
            ImGui.NextColumn();
            ImGuiEx.CenterColumnText("总卡片掉落价值", true);
            ImGui.NextColumn();
            if (stat.CardsWon.Count > 0)
            {
                ImGuiEx.CenterColumnText("赢得最多的卡片", true);
            }
            ImGui.NextColumn();
            ImGuiEx.CenterColumnText($"{stat.MGPWon.ToString("N0")} MGP");
            ImGui.NextColumn();
            ImGuiEx.CenterColumnText($"{GetDroppedCardValues(stat).ToString("N0")} MGP");
            ImGui.NextColumn();
            if (stat.CardsWon.Count > 0)
            {
                ImGuiEx.CenterColumnText($"{TriadCardDB.Get().FindById((int)stat.CardsWon.OrderByDescending(x => x.Value).First().Key).Name.GetLocalized()}");
                ImGui.NextColumn();
                ImGui.NextColumn();
                ImGui.NextColumn();
                ImGuiEx.CenterColumnText($"{stat.CardsWon.OrderByDescending(x => x.Value).First().Value.ToString("N0")} 次");
            }

            ImGui.Columns(1);
            ImGui.EndChild();
        }


        private int GetDroppedCardValues(Stats stat)
        {
            int output = 0;
            foreach (var card in stat.CardsWon)
                output += GameCardDB.Get().FindById((int)card.Key).SaleValue * stat.CardsWon[card.Key];

            return output;
        }

        public void DrawTriadTab()
        {
            bool enabled = TriadAutomater.ModuleEnabled;

            ImGui.TextWrapped(@"如何使用：挑战你想要玩卡牌的NPC。一旦开始挑战，点击""启用九宫幻卡模块""。");
            ImGui.Separator();

            if (ImGui.Checkbox("启用九宫幻卡模块", ref enabled))
            {
                TriadAutomater.ModuleEnabled = enabled;

                if (enabled)
                    CufModule.ModuleEnabled = false;
            }

            bool autoOpen = configuration.OpenAutomatically;

            if (ImGui.Checkbox("挑战NPC时自动打开Saucy", ref autoOpen))
            {
                configuration.OpenAutomatically = autoOpen;
                configuration.Save();
            }

            int selectedDeck = configuration.SelectedDeckIndex;

            if (Saucy.TTSolver.profileGS.GetPlayerDecks().Count() > 0)
            {
                if (!Saucy.Config.UseRecommendedDeck)
                {
                    ImGui.PushItemWidth(200);
                    string preview = "";

                    if (selectedDeck == -1 || Saucy.TTSolver.profileGS.GetPlayerDecks()[selectedDeck] is null)
                    {
                        preview = "";
                    }
                    else
                    {
                        preview = selectedDeck >= 0 ? Saucy.TTSolver.profileGS.GetPlayerDecks()[selectedDeck].name : string.Empty;
                    }

                    if (ImGui.BeginCombo("选择套牌", preview))
                    {
                        if (ImGui.Selectable(""))
                        {
                            configuration.SelectedDeckIndex = -1;
                        }

                        foreach (var deck in Saucy.TTSolver.profileGS.GetPlayerDecks())
                        {
                            if (deck is null) continue;
                            var index = deck.id;
                            if (ImGui.Selectable(deck.name, index == selectedDeck))
                            {
                                configuration.SelectedDeckIndex = index;
                                configuration.Save();
                            }
                        }

                        ImGui.EndCombo();
                    }

                    ImGui.SameLine();
                }
                bool useAutoDeck = Saucy.Config.UseRecommendedDeck;
                if (ImGui.Checkbox("自动选择胜率最高的套牌", ref useAutoDeck))
                {
                    Saucy.Config.UseRecommendedDeck = useAutoDeck;
                    Saucy.Config.Save();
                }
            }
            else
            {
                ImGui.TextWrapped("请先与NPC开始挑战来填充你的套牌列表。");
            }

            if (ImGui.Checkbox("玩X次", ref TriadAutomater.PlayXTimes) && (TriadAutomater.NumberOfTimes <= 0 || TriadAutomater.PlayUntilCardDrops || TriadAutomater.PlayUntilAllCardsDropOnce))
            {
                TriadAutomater.NumberOfTimes = 1;
                TriadAutomater.PlayUntilCardDrops = false;
                TriadAutomater.PlayUntilAllCardsDropOnce = false;
            }

            if (ImGui.Checkbox("直到任意卡牌掉落", ref TriadAutomater.PlayUntilCardDrops) && (TriadAutomater.NumberOfTimes <= 0 || TriadAutomater.PlayXTimes || TriadAutomater.PlayUntilAllCardsDropOnce))
            {
                TriadAutomater.NumberOfTimes = 1;
                TriadAutomater.PlayXTimes = false;
                TriadAutomater.PlayUntilAllCardsDropOnce = false;
            }

            if (GameNpcDB.Get().mapNpcs.TryGetValue(Saucy.TTSolver.preGameNpc?.Id ?? -1, out var npcInfo))
            {
                CurrentNPC = npcInfo;
            }
            else
            {
                CurrentNPC = null;
            }

            if (ImGui.Checkbox($"直到NPC掉落所有卡牌至少X次 {(CurrentNPC is null ? "" : $"({TriadNpcDB.Get().FindByID(CurrentNPC.npcId).Name.GetLocalized()})")}", ref TriadAutomater.PlayUntilAllCardsDropOnce))
            {
                TriadAutomater.TempCardsWonList.Clear();
                TriadAutomater.PlayUntilCardDrops = false;
                TriadAutomater.PlayXTimes = false;
                TriadAutomater.NumberOfTimes = 1;
            }

            bool onlyUnobtained = Saucy.Config.OnlyUnobtainedCards;

            if (TriadAutomater.PlayUntilAllCardsDropOnce)
            {
                ImGui.SameLine();
                if (ImGui.Checkbox("仅未获得的卡牌", ref onlyUnobtained))
                {
                    TriadAutomater.TempCardsWonList.Clear();
                    Saucy.Config.OnlyUnobtainedCards = onlyUnobtained;
                    Saucy.Config.Save();
                }
            }

            if (TriadAutomater.PlayUntilAllCardsDropOnce && CurrentNPC != null)
            {
                ImGui.Indent();
                GameCardDB.Get().Refresh();
                foreach (var card in CurrentNPC.rewardCards)
                {
                    if ((Saucy.Config.OnlyUnobtainedCards && !GameCardDB.Get().FindById(card).IsOwned) || !Saucy.Config.OnlyUnobtainedCards)
                    {
                        TriadAutomater.TempCardsWonList.TryAdd((uint)card, 0);
                        ImGui.Text($"- {TriadCardDB.Get().FindById((int)GameCardDB.Get().FindById(card).CardId).Name.GetLocalized()} {TriadAutomater.TempCardsWonList[(uint)card]}/{TriadAutomater.NumberOfTimes}");
                    }

                }

                if (Saucy.Config.OnlyUnobtainedCards && TriadAutomater.TempCardsWonList.Count == 0)
                {
                    ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.DalamudRed);
                    ImGui.TextWrapped("你已经从这个NPC那里获得了所有卡牌。除非取消勾选“仅未获得的卡牌”或选择不同的NPC，否则此功能将无法使用。");
                    ImGui.PopStyleColor();
                }
                ImGui.Unindent();
            }

            if (TriadAutomater.PlayXTimes || TriadAutomater.PlayUntilCardDrops || TriadAutomater.PlayUntilAllCardsDropOnce)
            {
                ImGui.PushItemWidth(150f);
                ImGui.Text("游戏次数:");
                ImGui.SameLine();

                if (ImGui.InputInt("###NumberOfTimes", ref TriadAutomater.NumberOfTimes))
                {
                    if (TriadAutomater.NumberOfTimes <= 0)
                        TriadAutomater.NumberOfTimes = 1;
                }

                ImGui.Checkbox("完成后登出", ref TriadAutomater.LogOutAfterCompletion);

                bool playSound = Saucy.Config.PlaySound;

                ImGui.Columns(2, null, false);
                if (ImGui.Checkbox("完成后播放声音", ref playSound))
                {
                    Saucy.Config.PlaySound = playSound;
                    Saucy.Config.Save();
                }

                if (playSound)
                {
                    ImGui.NextColumn();
                    ImGui.Text("选择声音");
                    if (ImGui.BeginCombo("###SelectSound", Saucy.Config.SelectedSound))
                    {
                        string path = Path.Combine(Svc.PluginInterface.AssemblyLocation.Directory.FullName, "Sounds");
                        foreach (var file in new DirectoryInfo(path).GetFiles())
                        {
                            if (ImGui.Selectable($"{Path.GetFileNameWithoutExtension(file.FullName)}", Saucy.Config.SelectedSound == Path.GetFileNameWithoutExtension(file.FullName)))
                            {
                                Saucy.Config.SelectedSound = Path.GetFileNameWithoutExtension(file.FullName);
                                Saucy.Config.Save();
                            }
                        }

                        ImGui.EndCombo();
                    }

                    if (ImGui.Button("打开声音文件夹"))
                    {
                        Process.Start("explorer.exe", @$"{Path.Combine(Svc.PluginInterface.AssemblyLocation.Directory.FullName, "Sounds")}");
                    }
                    ImGuiComponents.HelpMarker("将任何MP3文件放入声音文件夹以添加您自己的自定义声音。");
                }
                ImGui.Columns(1);
            }
        }


        public unsafe void DrawCufTab()
        {
            bool enabled = CufModule.ModuleEnabled;

            ImGui.TextWrapped(@"如何使用：点击""启用重击伽美蛇模块""然后走到重击伽美蛇机器前。");
            ImGui.Separator();

            if (ImGui.Checkbox("启用重击伽美蛇模块", ref enabled))
            {
                CufModule.ModuleEnabled = enabled;
                if (enabled && TriadAutomater.ModuleEnabled)
                    TriadAutomater.ModuleEnabled = false;
            }

            if (ImGui.Checkbox("玩X次", ref TriadAutomater.PlayXTimes) && TriadAutomater.NumberOfTimes <= 0)
            {
                TriadAutomater.NumberOfTimes = 1;
            }

            if (TriadAutomater.PlayXTimes)
            {
                ImGui.PushItemWidth(150f);
                ImGui.Text("次数：");
                ImGui.SameLine();

                if (ImGui.InputInt("###次数", ref TriadAutomater.NumberOfTimes))
                {
                    if (TriadAutomater.NumberOfTimes <= 0)
                        TriadAutomater.NumberOfTimes = 1;
                }

                ImGui.Checkbox("完成后登出", ref TriadAutomater.LogOutAfterCompletion);

                bool playSound = Saucy.Config.PlaySound;

                ImGui.Columns(2, null, false);
                if (ImGui.Checkbox("完成后播放声音", ref playSound))
                {
                    Saucy.Config.PlaySound = playSound;
                    Saucy.Config.Save();
                }

                if (playSound)
                {
                    ImGui.NextColumn();
                    ImGui.Text("选择声音");
                    if (ImGui.BeginCombo("###选择声音", Saucy.Config.SelectedSound))
                    {
                        string path = Path.Combine(Svc.PluginInterface.AssemblyLocation.Directory.FullName, "Sounds");
                        foreach (var file in new DirectoryInfo(path).GetFiles())
                        {
                            if (ImGui.Selectable($"{Path.GetFileNameWithoutExtension(file.FullName)}", Saucy.Config.SelectedSound == Path.GetFileNameWithoutExtension(file.FullName)))
                            {
                                Saucy.Config.SelectedSound = Path.GetFileNameWithoutExtension(file.FullName);
                                Saucy.Config.Save();
                            }
                        }

                        ImGui.EndCombo();
                    }

                    if (ImGui.Button("打开声音文件夹"))
                    {
                        Process.Start("explorer.exe", @$"{Path.Combine(Svc.PluginInterface.AssemblyLocation.Directory.FullName, "Sounds")}");
                    }
                    ImGuiComponents.HelpMarker("将任何MP3文件拖入声音文件夹，以添加您自己的自定义声音。");
                }
                ImGui.Columns(1);
            }
        }

    }
}
