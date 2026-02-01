using Myra.Graphics2D.UI;
using Microsoft.Xna.Framework;
using Myra.Graphics2D.Brushes;
using Myra.Graphics2D;
using System.Linq;
using System;
using System.Collections.Generic;

public class UIController
{
    public UIState CurrentUIState = UIState.None;
    public static World WorldInstance;
    private Desktop _desktop;

    private Label _dayLabel;
    private Label _goldLabel;
    private Label _debtLabel;

    private ConstructionView _constructionView = new();

    //Legacy Panels
    private VerticalStackPanel _rosterHeroPanel;
    private VerticalStackPanel _applicantsPanel;
    private VerticalStackPanel _partyPanel;
    private VerticalStackPanel _availableDungeonPanel;
    private VerticalStackPanel _rosterDungeonPanel;
    private VerticalStackPanel _missionLogPanel;
    private ScrollViewer _missionLogScroll;
    private Label _missionTimerLabel;
    private Button _missionActionBtn;


    Dungeon selectedDungeon;


    public void ChangeUIState(UIState newState)
    {
        if (CurrentUIState == newState) return;

        CurrentUIState = newState;
        if (WorldInstance != null)
        {
            WorldInstance.BuildUI();
        }
    }

    public void Draw()
    {
        _desktop.Render();
    }

    public void Build()
    {
        if (_desktop == null)
        {
            _desktop = new();
        }
        else
        {
            _desktop.Root = null;
        }

        var mainGrid = GridConfiguration();

        mainGrid.Widgets.Add(HeaderContainer());

        Widget content = null;

        switch (CurrentUIState)
        {
            case UIState.Building:
                content = _constructionView.Build(this);
                break;

            case UIState.Shopping:
                // Futuramente: content = _shoppingView.Build(this);
                break;

            case UIState.GuildHeroes:
                content = RosterContainer();
                break;

            case UIState.GuildApplicants:
                var splitPanel = new Grid { ColumnSpacing = 10 };

                var roster = RosterContainer();
                var applicants = ApplicantsContainer();

                Grid.SetColumn(roster, 0);
                Grid.SetColumn(applicants, 1);

                splitPanel.Widgets.Add(roster);
                splitPanel.Widgets.Add(applicants);
                content = splitPanel;
                break;

            case UIState.DungeonShopAndInventory:
                var dungeonSplit = new Grid { ColumnSpacing = 10 };

                var owned = RosterDungeonContainer();
                var shop = AvailableDungeonContainer();

                Grid.SetColumn(owned, 0);
                Grid.SetColumn(shop, 1);

                dungeonSplit.Widgets.Add(owned);
                dungeonSplit.Widgets.Add(shop);
                content = dungeonSplit;
                break;

            case UIState.MissionSelectParty:
                var missionSplit = new Grid { ColumnSpacing = 10 };

                var heroList = RosterContainer();
                var missionCtrl = PartyContainer();

                Grid.SetColumn(heroList, 0);
                Grid.SetColumn(missionCtrl, 1);

                missionSplit.Widgets.Add(heroList);
                missionSplit.Widgets.Add(missionCtrl);
                content = missionSplit;
                break;
        }

        if (content != null)
        {
            Grid.SetRow(content, 1);
            Grid.SetColumn(content, 0);
            Grid.SetColumnSpan(content, 2);
            mainGrid.Widgets.Add(content);
        }

        _desktop.Root = mainGrid;

        RefreshInfo();
    }

    public Grid GridConfiguration()
    {
        var grid = new Grid
        {
            RowSpacing = 10,
            ColumnSpacing = 10,
            Padding = new Myra.Graphics2D.Thickness(10)
        };

        grid.ColumnsProportions.Add(new Proportion(ProportionType.Part));
        grid.ColumnsProportions.Add(new Proportion(ProportionType.Part));
        grid.RowsProportions.Add(new Proportion(ProportionType.Auto));
        grid.RowsProportions.Add(new Proportion(ProportionType.Fill));

        return grid;
    }

    public HorizontalStackPanel HeaderContainer()
    {
        var headerContainer = new HorizontalStackPanel { Spacing = 20 };
        var title = new Label { Text = $"Guild: {WorldInstance.Guild.Name}" };
        _dayLabel = new Label { Text = $"{WorldInstance.WorldTimeManager.CurrentTime}" };
        _goldLabel = new Label { Text = $"Gold: {WorldInstance.Guild.Gold}", TextColor = Color.Gold };
        _debtLabel = new Label { Text = $"Debt: {WorldInstance.Guild.Debt}", TextColor = Color.Red };
        var borrowButton = new Button
        {
            Content = new Label { Text = "borrow", TextColor = Color.Yellow }
        };
        borrowButton.Click += (s, a) =>
        {
            if (WorldInstance.Guild.Debt <= 1000)
            {
                WorldInstance.Guild.Gold += 500;
                WorldInstance.Guild.Debt += 500;
            }
            RefreshInfo();
        };
        var payButton = new Button
        {
            Content = new Label { Text = "pay", TextColor = Color.Green }
        };
        payButton.Click += (s, a) =>
        {
            if (WorldInstance.Guild.Gold >= WorldInstance.Guild.Debt)
            {
                WorldInstance.Guild.Gold -= WorldInstance.Guild.Debt;
                WorldInstance.Guild.Debt = 0;
            }
            else if (WorldInstance.Guild.Gold >= 500)
            {
                WorldInstance.Guild.Gold -= 500;
                WorldInstance.Guild.Debt -= 500;
            }
            else if (WorldInstance.Guild.Gold > 0)
            {
                WorldInstance.Guild.Debt -= WorldInstance.Guild.Gold;
                WorldInstance.Guild.Gold = 0;
            }
            RefreshInfo();
        };
        headerContainer.Widgets.Add(title);
        headerContainer.Widgets.Add(_dayLabel);
        headerContainer.Widgets.Add(_goldLabel);
        headerContainer.Widgets.Add(_debtLabel);
        headerContainer.Widgets.Add(borrowButton);
        headerContainer.Widgets.Add(payButton);

        Grid.SetRow(headerContainer, 0);
        Grid.SetColumn(headerContainer, 0);
        Grid.SetColumnSpan(headerContainer, 2);
        return headerContainer;
    }

    public Widget RosterContainer()
    {
        _rosterHeroPanel = new VerticalStackPanel { Spacing = 5 };
        var container = new Grid { RowSpacing = 5 };

        if (CurrentUIState == UIState.GuildHeroes || CurrentUIState == UIState.MissionSelectParty || CurrentUIState == UIState.GuildApplicants)
        {
            container.RowsProportions.Add(new Proportion(ProportionType.Auto));
            container.RowsProportions.Add(new Proportion(ProportionType.Fill));

            var rosterTitle = new Label { Text = "Current Heroes" };
            container.Widgets.Add(rosterTitle);

            var rosterScroll = new ScrollViewer();
            rosterScroll.Content = _rosterHeroPanel;
            
            Grid.SetRow(rosterScroll, 1);
            container.Widgets.Add(rosterScroll);
        }
        return container;
    }

    public Widget ApplicantsContainer()
    {
        _applicantsPanel = new VerticalStackPanel { Spacing = 5 };
        var container = new Grid { RowSpacing = 5 };

        if (CurrentUIState != UIState.GuildApplicants) return container;

        container.RowsProportions.Add(new Proportion(ProportionType.Auto));
        container.RowsProportions.Add(new Proportion(ProportionType.Fill));

        var applicantsTitle = new Label { Text = "Hire New Heroes" };
        container.Widgets.Add(applicantsTitle);
        
        var applicantsScroll = new ScrollViewer();
        applicantsScroll.Content = _applicantsPanel;

        Grid.SetRow(applicantsScroll, 1);
        container.Widgets.Add(applicantsScroll);
        
        return container;
    }

    public Widget PartyContainer()
    {
        _partyPanel = new VerticalStackPanel { Spacing = 5 };
        _missionLogPanel = new VerticalStackPanel
        {
            HorizontalAlignment = HorizontalAlignment.Stretch
        };

        var container = new Grid { RowSpacing = 5 };

        if (CurrentUIState == UIState.MissionSelectParty)
        {
            int currentRow = 0;

            container.RowsProportions.Add(new Proportion(ProportionType.Auto));
            var containerTittle = new Label { Text = (selectedDungeon != null) ? $"Mission Control" : "Select a Dungeon First" };
            container.Widgets.Add(containerTittle);
            currentRow++;

            container.RowsProportions.Add(new Proportion(ProportionType.Part));

            var partyScroll = new ScrollViewer();
            partyScroll.Content = _partyPanel;
            Grid.SetRow(partyScroll, currentRow);
            container.Widgets.Add(partyScroll);
            currentRow++;            

            if (selectedDungeon != null)
            {
                container.RowsProportions.Add(new Proportion(ProportionType.Auto));

                _missionTimerLabel = new Label
                {
                    Text = "Status: Idle",
                    TextColor = Color.Yellow,
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                Grid.SetRow(_missionTimerLabel, currentRow);
                container.Widgets.Add(_missionTimerLabel);
                currentRow++;

                container.RowsProportions.Add(new Proportion(ProportionType.Auto));

                var logLabel = new Label { Text = "Mission Logs:", TextColor = Color.Gray };
                Grid.SetRow(logLabel, currentRow);
                container.Widgets.Add(logLabel);
                currentRow++;

                container.RowsProportions.Add(new Proportion(ProportionType.Pixels, 150));

                _missionLogScroll = new ScrollViewer
                {
                    Background = new SolidBrush(new Color(0, 0, 0, 100)),
                    VerticalAlignment = VerticalAlignment.Stretch
                };
                _missionLogScroll.Content = _missionLogPanel;

                Grid.SetRow(_missionLogScroll, currentRow);
                container.Widgets.Add(_missionLogScroll);
                currentRow++;
            }
        }
        return container;
    }

    public Widget AvailableDungeonContainer()
    {
        _availableDungeonPanel = new VerticalStackPanel { Spacing = 5 };
        var container = new Grid {RowSpacing = 5};

        if (CurrentUIState != UIState.DungeonShopAndInventory) return container;

        container.RowsProportions.Add(new Proportion(ProportionType.Auto));
        container.RowsProportions.Add(new Proportion(ProportionType.Fill));

        var dungeonTitle = new Label { Text = "Available Dungeons" };
        container.Widgets.Add(dungeonTitle);
        
        var dungeonScroll = new ScrollViewer();
        dungeonScroll.Content = _availableDungeonPanel;

        Grid.SetRow(dungeonScroll, 1);
        container.Widgets.Add(dungeonScroll);

        return container;
    }

    public Widget RosterDungeonContainer()
    {
        _rosterDungeonPanel = new VerticalStackPanel { Spacing = 5 };
        var container = new Grid{RowSpacing = 5};

        if (CurrentUIState != UIState.DungeonShopAndInventory) return container;

        container.RowsProportions.Add(new Proportion(ProportionType.Auto));
        container.RowsProportions.Add(new Proportion(ProportionType.Fill));

        var dungeonTitle = new Label { Text = "Owned Dungeons" };
        container.Widgets.Add(dungeonTitle);
        
        var dungeonScroll = new ScrollViewer();
        dungeonScroll.Content = _rosterDungeonPanel;
        container.Widgets.Add(dungeonScroll);

        Grid.SetRow(dungeonScroll, 1);

        return container;
    }

    public void UpdateRealtimeValues()
    {
        if (WorldInstance == null) return;
        if (_dayLabel != null)
        {
            _dayLabel.Text = $"Day: {WorldInstance.WorldTimeManager.CurrentTime.ToString("hh:mm dd MMM yyyy")}";
            _goldLabel.Text = $"Gold: {WorldInstance.Guild.Gold}";
            _debtLabel.Text = $"Debt: {WorldInstance.Guild.Debt}";
        }

        if (selectedDungeon != null && _missionTimerLabel != null && selectedDungeon.Mission != null)
        {
            TimeSpan remaining = selectedDungeon.NextActionTime - WorldInstance.WorldTimeManager.CurrentTime;
            if (selectedDungeon.DungeonState == DungeonState.Cleaning || selectedDungeon.DungeonState == DungeonState.Collecting)
            {
                if (remaining.TotalSeconds > 0)
                {
                    _missionTimerLabel.Text = $"Action: {selectedDungeon.DungeonState}";

                    if (_missionActionBtn != null)
                    {
                        _missionActionBtn.Enabled = false;
                        if (_missionActionBtn.Content is Label l) l.Text = "Processing...";
                    }
                }
                else
                {
                    if (selectedDungeon.DungeonState == DungeonState.Completed)
                    {
                        if (remaining.TotalSeconds <= 0)
                        {
                            _missionTimerLabel.Text = "Mission Completed! Claim Rewards.";
                            _missionTimerLabel.TextColor = Color.LightGreen;

                            if (_missionActionBtn != null)
                            {
                                if (!_missionActionBtn.Enabled || (_missionActionBtn.Content is Label l && l.Text != "Claim Rewards"))
                                {
                                    _missionActionBtn.Enabled = true;
                                    if (_missionActionBtn.Content is Label lbl) lbl.Text = "Claim Rewards";
                                }
                            }
                        }
                    }
                }
            }
            else if (selectedDungeon.DungeonState == DungeonState.Completed)
            {
                if (remaining.TotalSeconds <= 0)
                {
                    _missionTimerLabel.Text = "Mission Completed! Claim Rewards.";
                    _missionTimerLabel.TextColor = Color.LightGreen;

                    if (_missionActionBtn != null)
                    {
                        if (!_missionActionBtn.Enabled || (_missionActionBtn.Content is Label l && l.Text != "Claim Rewards"))
                        {
                            _missionActionBtn.Enabled = true;
                            if (_missionActionBtn.Content is Label lbl) lbl.Text = "Claim Rewards";
                        }
                    }
                }
                else
                {
                    _missionTimerLabel.Text = $"Transporting Loot...";
                    if (_missionActionBtn != null)
                    {
                        _missionActionBtn.Enabled = false;
                        if (_missionActionBtn.Content is Label l) l.Text = "Returning...";
                    }
                }
            }

            else if (selectedDungeon.DungeonState == DungeonState.Failed)
            {
                _missionTimerLabel.Text = "Mission FAILED.";
                _missionTimerLabel.TextColor = Color.Red;
                RefreshInfo();

                if (_missionActionBtn != null && !_missionActionBtn.Enabled)
                {
                    _missionActionBtn.Enabled = true;
                    if (_missionActionBtn.Content is Label l) l.Text = "Start Mission";
                }
                selectedDungeon.DungeonState = DungeonState.NotStarted;
            }

            if (_missionLogPanel != null && selectedDungeon.Logs.Count > _missionLogPanel.Widgets.Count)
            {
                UpdateMissionLogsUI();
                RefreshInfo();
            }
        }
    }

    private void UpdateMissionLogsUI()
    {
        if (selectedDungeon == null || _missionLogPanel == null) return;

        int CurrentUICount = _missionLogPanel.Widgets.Count;
        int totalLogs = selectedDungeon.Logs.Count;

        for (int i = CurrentUICount; i < totalLogs; i++)
        {
            var rawLog = selectedDungeon.Logs[i];

            (string formattedText, Color logColor) = FormatLogAndGetColor(rawLog);

            var label = new Label
            {
                Text = formattedText,
                Wrap = true,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                TextColor = logColor,
            };

            _missionLogPanel.Widgets.Add(label);
        }

        if (_missionLogScroll != null) _missionLogScroll.ScrollPosition = new Point(0, int.MaxValue);
    }

    private (string, Color) FormatLogAndGetColor(string log)
    {
        string text = log;
        Color color = Color.White;

        text = text.Replace("🏰", "[HERO] ");
        text = text.Replace("🛑", "[ENEMY] ");

        text = text.Replace("⚔️", "[ATK]");
        text = text.Replace("🔥", "[MAG]");
        text = text.Replace("💚", "[HEAL]");
        text = text.Replace("⛓️‍💥", "[MISS]");
        text = text.Replace("💦", "[TIRED]");

        text = text.Replace("☠️", "[DEAD]");
        text = text.Replace("💀", "[DEAD]");
        text = text.Replace("⚠️", "[WARN]");
        text = text.Replace("✨", "[WIN]");
        text = text.Replace("✅", "[CLEAR]");
        text = text.Replace("💰", "[GOLD]");
        text = text.Replace("📦", "[LOOT]");
        text = text.Replace("⛺", "[REST]");
        text = text.Replace("📊", "[STATS]");

        if (log.Contains("Crit"))
        {
            color = Color.Yellow;
            text = text.ToUpper();
        }
        else if (text.Contains("[HEAL]"))
        {
            color = Color.LightGreen;
        }
        else if (text.Contains("[MISS]"))
        {
            color = Color.Gray;
        }
        else if (text.Contains("[DEAD]") || text.Contains("morreu") || text.Contains("caiu"))
        {
            color = Color.Red;
        }
        else if (text.Contains("[WARN]"))
        {
            color = Color.Orange;
        }
        else if (text.Contains("[WIN]") || text.Contains("[CLEAR]"))
        {
            color = Color.Gold;
        }
        else if (text.Contains("[GOLD]") || text.Contains("[LOOT]"))
        {
            color = Color.LightGoldenrodYellow;
        }
        else if (text.Contains("[ATK]") || text.Contains("[MAG]"))
        {
            if (text.StartsWith("[HERO]")) color = Color.LightBlue;
            else color = Color.IndianRed;
        }
        else if (text.Contains("[TIRED]") || text.Contains("[REST]"))
        {
            color = Color.LightSkyBlue;
        }

        return (text, color);
    }

    public void RefreshInfo()
    {
        _rosterHeroPanel?.Widgets.Clear();
        _applicantsPanel?.Widgets.Clear();
        _rosterDungeonPanel?.Widgets.Clear();
        _availableDungeonPanel?.Widgets.Clear();
        _partyPanel?.Widgets.Clear();
        _missionLogPanel?.Widgets.Clear();

        if (CurrentUIState == UIState.Building)
        {
            _constructionView.Refresh();
        }
        else
        {
            LegacyRefreshLogic();
        }
    }

    private void LegacyRefreshLogic()
    {
        if (_partyPanel != null && selectedDungeon != null)
        {
            string buttonText = "";
            bool isButtonActive = true;

            switch (selectedDungeon.DungeonState)
            {
                case DungeonState.NotStarted:
                    buttonText = "Start Mission";
                    break;
                case DungeonState.Cleaning:
                case DungeonState.Collecting:
                case DungeonState.Resting:
                    buttonText = "In Progress...";
                    isButtonActive = false;
                    break;
                case DungeonState.Completed:
                    buttonText = "Claim Rewards";
                    isButtonActive = true;
                    break;
                case DungeonState.Failed:
                    buttonText = "Start Mission";
                    isButtonActive = true;
                    break;
            }

            _missionActionBtn = new Button
            {
                Padding = new Thickness(5),
                HorizontalAlignment = HorizontalAlignment.Center,
                Content = new Label { Text = buttonText },
                Enabled = isButtonActive,
            };

            _missionActionBtn.Click += (s, a) =>
            {
                if (selectedDungeon.DungeonState == DungeonState.NotStarted || selectedDungeon.DungeonState == DungeonState.Failed)
                {
                    if (selectedDungeon.Mission.Party.Count > 0)
                    {
                        WorldInstance.Guild.StartMission(selectedDungeon.Mission);
                        RefreshInfo();
                    }
                }
                else if (selectedDungeon.DungeonState == DungeonState.Completed)
                {
                    selectedDungeon.CollectRewards();
                    WorldInstance.Guild.Missions.Remove(selectedDungeon.Mission);
                    WorldInstance.Guild.OwnedDungeons.Remove(selectedDungeon);
                    selectedDungeon.Mission = null;
                    selectedDungeon.DungeonState = DungeonState.NotStarted;
                    selectedDungeon = null;
                    Build();
                }
                else if (selectedDungeon.DungeonState == DungeonState.Failed)
                {
                    selectedDungeon.DungeonLoot.Clear();

                    WorldInstance.Guild.Missions.Remove(selectedDungeon.Mission);
                    selectedDungeon.Mission = null;
                    selectedDungeon.ResetDungeon();
                    selectedDungeon = null;
                    RefreshInfo();
                }
            };
            _partyPanel.Widgets.Add(_missionActionBtn);
        }

        if (_rosterHeroPanel != null)
        {
            foreach (var hero in WorldInstance.Guild.Heroes)
            {
                if (hero.HP > 0)
                {
                    var card = CreateHeroCard(hero, CardType.GuildHero);
                    _rosterHeroPanel.Widgets.Add(card);
                }
            }
        }

        if (_applicantsPanel != null)
        {
            foreach (var applicant in WorldInstance.Guild.Applicants)
            {
                var card = CreateHeroCard(applicant, CardType.Applicant);
                _applicantsPanel.Widgets.Add(card);
            }
        }

        if (_partyPanel != null && selectedDungeon != null)
        {
            var dungeonCard = CreateDungeonCard(selectedDungeon, true);
            _partyPanel.Widgets.Add(dungeonCard);
            if (selectedDungeon.Mission != null)
            {
                foreach (var party in selectedDungeon.Mission.Party)
                {
                    var card = CreateHeroCard(party, CardType.PartyHero);
                    _partyPanel.Widgets.Add(card);
                }
            }
            if (_missionLogPanel != null) UpdateMissionLogsUI();
        }

        if (_rosterDungeonPanel != null)
        {
            foreach (var guildDungeon in WorldInstance.Guild.OwnedDungeons)
            {
                var card = CreateDungeonCard(guildDungeon, true);
                _rosterDungeonPanel.Widgets.Add(card);
            }
        }

        if (_availableDungeonPanel != null)
        {
            foreach (var dungeon in WorldInstance.Guild.AvailableDungeons)
            {
                var card = CreateDungeonCard(dungeon, false);
                _availableDungeonPanel.Widgets.Add(card);
            }
        }
    }

    enum CardType
    {
        Applicant,
        GuildHero,
        PartyHero,
    }

    private Panel CreateHeroCard(Hero hero, CardType cardType)
    {
        var panel = new Panel
        {
            Padding = new Thickness(10),
            Background = new SolidBrush(new Color(30, 30, 30, 200)),
            Border = new SolidBrush(Color.Gray),
            BorderThickness = new Thickness(1),
        };

        var grid = new Grid { ColumnSpacing = 10 };
        grid.ColumnsProportions.Add(new Proportion(ProportionType.Fill));
        grid.ColumnsProportions.Add(new Proportion(ProportionType.Auto));
        grid.RowsProportions.Add(new Proportion(ProportionType.Auto));

        var infoStack = new VerticalStackPanel();

        var nameLabel = new Label { Text = $"{hero.Name} | Satisfaction: {hero.Satisfaction}" };
        if (hero.Stressed) nameLabel.TextColor = Color.OrangeRed;

        infoStack.Widgets.Add(nameLabel);
        infoStack.Widgets.Add(new Label { Text = $"{hero.Class} | Wage: {hero.Wage}g", TextColor = Color.LightGray });

        var elementPanel = new HorizontalStackPanel { Spacing = 5 };
        var typeLabel = new Label { Text = $"Elem: {hero.Element.Type}", TextColor = Color.Cyan };
        elementPanel.Widgets.Add(typeLabel);

        infoStack.Widgets.Add(elementPanel);

        var statsLabel = new Label { Text = $"HP: {hero.HP}/{hero.MaxHP} | Energy: {hero.Energy}/{hero.MaxEnergy} | Mana: {hero.Mana}/{hero.MaxMana}" };
        if (hero.HP < hero.MaxHP * 0.3f) statsLabel.TextColor = Color.Red;
        infoStack.Widgets.Add(statsLabel);

        infoStack.Widgets.Add(new Label { Text = $"STR: {hero.Strength} | DEX: {hero.Dexterity} | CON: {hero.Constitution} | WIS: {hero.Wisdom} | CHAR: {hero.Charisma}" });
        infoStack.Widgets.Add(new Label { Text = $"Dmg: {hero.PhysicalDamage} | Magic: {hero.MagicPower} | Arm: {hero.Armour}" });

        if (hero.Debt > 0) infoStack.Widgets.Add(new Label { Text = $"Debt: {hero.Debt}", TextColor = Color.Red });

        Grid.SetColumn(infoStack, 0);
        grid.Widgets.Add(infoStack);

        var buttonStack = new VerticalStackPanel { Spacing = 5, VerticalAlignment = VerticalAlignment.Center };
        var buttonText = "";
        float hireMultiplier = 1f;
        switch (cardType)
        {
            case CardType.Applicant: buttonText = $"Hire {(int)(hero.Wage * hireMultiplier)}g"; break;
            case CardType.GuildHero: buttonText = (CurrentUIState == UIState.MissionSelectParty) ? "Add" : "Fire"; break;
            case CardType.PartyHero: buttonText = "Remove"; break;
        }

        bool isButtonActive = true;
        if (CurrentUIState == UIState.MissionSelectParty)
        {
            if (selectedDungeon != null) isButtonActive = (selectedDungeon.Mission != null);
            else isButtonActive = false;
        }

        var actionButton = new Button
        {
            Padding = new Thickness(5),
            Content = new Label { Text = buttonText, HorizontalAlignment = HorizontalAlignment.Center },
            Enabled = isButtonActive,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };

        switch (cardType)
        {
            default:
                break;
            case CardType.Applicant:
                actionButton.Click += (s, a) =>
                {
                    var value = (int)(hero.Wage * hireMultiplier);
                    if (WorldInstance.Guild.Gold >= value)
                    {
                        WorldInstance.Guild.Hire(hero);
                        WorldInstance.Guild.Gold -= value;
                        WorldInstance.Guild.Applicants.Remove(hero);
                    }
                    RefreshInfo();
                };
                break;
            case CardType.GuildHero:
                actionButton.Click += (s, a) =>
                {
                    if (CurrentUIState == UIState.MissionSelectParty)
                    {
                        if (selectedDungeon != null)
                        {
                            selectedDungeon.Mission.AddToParty(hero);
                        }
                    }
                    else
                    {
                        WorldInstance.Guild.Fire(hero);
                    }
                    RefreshInfo();
                };
                break;
            case CardType.PartyHero:
                actionButton.Click += (s, a) =>
                {
                    selectedDungeon.Mission.RemoveParty(hero);
                    RefreshInfo();
                };
                break;
        }
        buttonStack.Widgets.Add(actionButton);

        if (CurrentUIState == UIState.GuildHeroes || CurrentUIState == UIState.GuildApplicants)
        {
            bool hasDebt = hero.Debt > 0;

            if (cardType == CardType.GuildHero)
            {
                var payButton = new Button
                {
                    Padding = new Thickness(5),
                    Background = new SolidBrush(new Color(0, 100, 0, 200)),
                    Border = new SolidBrush(Color.Green),
                    BorderThickness = new Thickness(1),
                    Enabled = hasDebt,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    Content = new Label
                    {
                        Text = hasDebt ? $"Pay: {hero.Debt}g" : "Paid",
                        TextColor = hasDebt ? Color.Red : Color.LightGreen,
                        HorizontalAlignment = HorizontalAlignment.Center
                    }
                };

                payButton.Click += (s, a) =>
                {
                    WorldInstance.Guild.PayHero(hero);
                    RefreshInfo();
                };

                buttonStack.Widgets.Add(payButton);
            }
        }

        Grid.SetColumn(buttonStack, 1);
        grid.Widgets.Add(buttonStack);

        panel.Widgets.Add(grid);
        return panel;
    }

    private Panel CreateDungeonCard(Dungeon dungeon, bool IsOwned)
    {
        var panel = new Panel()
        {
            Padding = new Thickness(10),
            Background = new SolidBrush(new Color(30, 30, 30, 200)),
            Border = new SolidBrush(Color.Gray),
            BorderThickness = new Myra.Graphics2D.Thickness(1),
        };

        if (dungeon == selectedDungeon)
        {
            panel.Border = new SolidBrush(Color.Yellow);
            panel.BorderThickness = new Thickness(2);
        }

        var grid = new Grid { ColumnSpacing = 10 };
        grid.ColumnsProportions.Add(new Proportion(ProportionType.Fill));
        grid.ColumnsProportions.Add(new Proportion(ProportionType.Auto));
        grid.RowsProportions.Add(new Proportion(ProportionType.Auto));

        var dungeonInfo = new VerticalStackPanel();
        dungeonInfo.Widgets.Add(new Label { Text = $"{dungeon.Biome.Type} (Lvl {dungeon.Level})", TextColor = Color.Cyan });
        var elementsList = dungeon.Biome.BiomeCurrentElements;
        var elementsText = string.Join(" , ", elementsList);
        dungeonInfo.Widgets.Add(new Label { Text = "Elements: " + elementsText, TextColor = Color.IndianRed });

        HashSet<ElementType> recommended = new HashSet<ElementType>();
        foreach (var elem in elementsList)
        {
            var tempInfo = new ElementInfo(elem);
            foreach (var weakness in tempInfo.WeakAgainst)
            {
                recommended.Add(weakness);
            }
        }

        if (recommended.Count > 0)
        {
            string recText = GetShortElementList(recommended.ToList());
            if (CurrentUIState == UIState.MissionSelectParty)
            {
                dungeonInfo.Widgets.Add(new Label { Text = $"Bring: {recText}", TextColor = Color.LightGreen });
            }
        }

        if (IsOwned)
        {
            var resourcesList = dungeon.DungeonResources.Resources.Select(r => $"{r.Type}: {r.Quantity}");
            dungeonInfo.Widgets.Add(new Label { Text = $"Rooms: {dungeon.Rooms.Count}" });
            int totalValue = dungeon.DungeonResources.Resources.Sum(r => r.Price * r.Quantity);
            dungeonInfo.Widgets.Add(new Label { Text = string.Join(" | ", resourcesList), Wrap = true });
            dungeonInfo.Widgets.Add(new Label { Text = $"Est. Value: {totalValue}g", TextColor = Color.Green });
        }
        else
        {
            int totalValue = dungeon.DungeonResources.Resources.Sum(r => r.Price * r.Quantity);
            int profitMargin = totalValue - dungeon.Price;

            dungeonInfo.Widgets.Add(new Label { Text = $"Rooms: {dungeon.Rooms.Count}" });
            dungeonInfo.Widgets.Add(new Label { Text = $"Cost: {dungeon.Price}g", TextColor = Color.Red });
        }
        Grid.SetColumn(dungeonInfo, 0);
        grid.Widgets.Add(dungeonInfo);

        if (CurrentUIState != UIState.MissionSelectParty)
        {
            var actionButton = new Button
            {
                Padding = new Thickness(5),
                Content = new Label
                {
                    Text = !IsOwned ? $"Buy: {dungeon.Price}g" : $"View",
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                },
            };


            actionButton.Click += (s, a) =>
            {
                var guild = WorldInstance.Guild;
                if (!IsOwned)
                {
                    if (WorldInstance.Guild.Gold >= dungeon.Price)
                    {
                        guild.BuyDungeon(dungeon);
                        guild.Gold -= dungeon.Price;
                        RefreshInfo();
                    }
                }
                else
                {
                    selectedDungeon = dungeon;
                    if (selectedDungeon.Mission == null)
                    {
                        selectedDungeon.Mission = new MissionManager(WorldInstance, selectedDungeon);
                    }
                    WorldInstance.Guild.Missions.Add(dungeon.Mission);
                    ChangeUIState(UIState.MissionSelectParty);
                }
            };
            Grid.SetColumn(actionButton, 1);
            grid.Widgets.Add(actionButton);
        }

        panel.Widgets.Add(grid);

        return panel;
    }

    private string GetShortElementList(List<ElementType> types)
    {
        if (types.Count == 0) return "None";
        return string.Join(", ", types.Select(t => t.ToString()));
    }

    public bool IsMouseOverGUI()
    {
        return _desktop != null && _desktop.IsMouseOverGUI;
    }
}