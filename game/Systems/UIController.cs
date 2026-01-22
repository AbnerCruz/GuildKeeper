using Myra.Graphics2D.UI;
using Microsoft.Xna.Framework;
using Myra.Graphics2D.Brushes;
using Myra.Graphics2D;
using System.Linq;
using System;

public class UIController
{
    public UIState CurrentUIState = UIState.None;
    public static World WorldInstance;
    private Desktop _desktop;

    private Label _dayLabel;
    private Label _goldLabel;
    private Label _debtLabel;
    private VerticalStackPanel _rosterHeroPanel;
    private VerticalStackPanel _applicantsPanel;
    private VerticalStackPanel _partyPanel;
    private VerticalStackPanel _availableDungeonPanel;
    private VerticalStackPanel _rosterDungeonPanel;

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

        var grid = GridConfiguration();

        grid.Widgets.Add(HeaderContainer());
        grid.Widgets.Add(RosterContainer());
        grid.Widgets.Add(ApplicantsContainer());
        grid.Widgets.Add(AvailableDungeonContainer());
        grid.Widgets.Add(RosterDungeonContainer());
        grid.Widgets.Add(PartyContainer());

        _desktop.Root = grid;

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
        var title = new Label { Text = $"Guild: {WorldInstance.Guild.Name}", Scale = new Vector2(1) };
        _dayLabel = new Label { Text = $"{WorldInstance.WorldTimeManager.CurrentTime}" };
        _goldLabel = new Label { Text = $"Gold: {WorldInstance.Guild.Gold}", TextColor = Color.Gold, Scale = new Vector2(1) };
        _debtLabel = new Label { Text = $"Debt: {WorldInstance.Guild.Debt}", TextColor = Color.Red, Scale = new Vector2(1) };
        var borrowButton = new Button
        {
            Content = new Label { Text = "borrow", TextColor = Color.Yellow}
        };
        borrowButton.Click += (s, a) =>
        {
            if(WorldInstance.Guild.Debt <= 1000)
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

    public VerticalStackPanel RosterContainer()
    {
        _rosterHeroPanel = new VerticalStackPanel { Spacing = 5 };
        var rosterContainer = new VerticalStackPanel();

        if (CurrentUIState == UIState.GuildHeroes || CurrentUIState == UIState.MissionSelectParty || CurrentUIState == UIState.GuildApplicants)
        {
            var rosterScrool = new ScrollViewer();
            rosterScrool.Content = _rosterHeroPanel;

            var rosterTitle = new Label { Text = "Current Heroes" };
            rosterContainer.Widgets.Add(rosterTitle);
            rosterContainer.Widgets.Add(rosterScrool);

            Grid.SetRow(rosterContainer, 1);
            Grid.SetColumn(rosterContainer, 0);
        }
        return rosterContainer;
    }

    public VerticalStackPanel ApplicantsContainer()
    {
        _applicantsPanel = new VerticalStackPanel { Spacing = 5 };
        var applicantsContainer = new VerticalStackPanel();

        if (CurrentUIState != UIState.GuildApplicants) return applicantsContainer;

        var applicantsScroll = new ScrollViewer();
        applicantsScroll.Content = _applicantsPanel;

        var applicantsTitle = new Label { Text = "Hire New Heroes" };
        applicantsContainer.Widgets.Add(applicantsTitle);
        applicantsContainer.Widgets.Add(applicantsScroll);

        Grid.SetRow(applicantsContainer, 1);
        Grid.SetColumn(applicantsContainer, 1);
        return applicantsContainer;
    }

    public VerticalStackPanel PartyContainer()
    {
        _partyPanel = new VerticalStackPanel { Spacing = 5 };
        var partyContainer = new VerticalStackPanel();

        if (CurrentUIState == UIState.MissionSelectParty)
        {
            var partyScroll = new ScrollViewer();
            partyScroll.Content = _partyPanel;

            var ContainerTittle = new Label { Text = (selectedDungeon != null) ? $"Select a Party" : "Select an Dungeon First" };
            partyContainer.Widgets.Add(ContainerTittle);
            partyContainer.Widgets.Add(partyScroll);

            Grid.SetRow(partyContainer, 1);
            Grid.SetColumn(partyContainer, 1);
        }
        return partyContainer;
    }

    public VerticalStackPanel AvailableDungeonContainer()
    {
        _availableDungeonPanel = new VerticalStackPanel { Spacing = 5 };
        var dungeonContainer = new VerticalStackPanel();

        if (CurrentUIState != UIState.DungeonShopAndInventory) return dungeonContainer;

        var dungeonScroll = new ScrollViewer();
        dungeonScroll.Content = _availableDungeonPanel;
        var dungeonTitle = new Label { Text = "Available Dungeons" };
        dungeonContainer.Widgets.Add(dungeonTitle);
        dungeonContainer.Widgets.Add(dungeonScroll);

        Grid.SetRow(dungeonContainer, 1);
        Grid.SetColumn(dungeonContainer, 1);

        return dungeonContainer;
    }

    public VerticalStackPanel RosterDungeonContainer()
    {
        _rosterDungeonPanel = new VerticalStackPanel { Spacing = 5 };
        var Container = new VerticalStackPanel();

        if (CurrentUIState != UIState.DungeonShopAndInventory) return Container;

        var dungeonScroll = new ScrollViewer();
        dungeonScroll.Content = _rosterDungeonPanel;
        var dungeonTitle = new Label { Text = "Owned Dungeons" };
        Container.Widgets.Add(dungeonTitle);
        Container.Widgets.Add(dungeonScroll);

        Grid.SetRow(Container, 1);
        Grid.SetColumn(Container, 0);

        return Container;
    }

    public void UpdateRealtimeValues()
    {
        if (_dayLabel == null || WorldInstance == null) return;

        _dayLabel.Text = $"Day: {WorldInstance.WorldTimeManager.CurrentTime.ToString("dd MMM yyyy")}";
        _goldLabel.Text = $"Gold: {WorldInstance.Guild.Gold}";
        _debtLabel.Text = $"Debt: {WorldInstance.Guild.Debt}";
    }

    public void RefreshInfo()
    {
        _rosterHeroPanel.Widgets.Clear();
        _applicantsPanel.Widgets.Clear();
        _rosterDungeonPanel.Widgets.Clear();
        _availableDungeonPanel.Widgets.Clear();
        _partyPanel.Widgets.Clear();

        foreach (var hero in WorldInstance.Guild.Heroes)
        {
            var card = CreateHeroCard(hero, CardType.GuildHero);
            _rosterHeroPanel.Widgets.Add(card);
        }

        foreach (var applicant in WorldInstance.Guild.Applicants)
        {
            var card = CreateHeroCard(applicant, CardType.Applicant);
            _applicantsPanel.Widgets.Add(card);
        }

        if (selectedDungeon != null)
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
            string buttonText = "";
            bool isButtonActive = true;
            switch (selectedDungeon.DungeonState)
            {
                case DungeonState.Completed:
                    if (selectedDungeon.Mission != null)
                    {
                        buttonText = "Claim Rewards";
                        isButtonActive = true;
                    }
                    else
                    {
                        buttonText = "Mission Completed";
                        isButtonActive = false;
                    }
                    break;
                case DungeonState.NotStarted:
                    buttonText = "Start Mission";
                    isButtonActive = true;
                    break;
                case DungeonState.Cleaning:
                case DungeonState.Collecting:
                    buttonText = "In Progress...";
                    isButtonActive = false;
                    break;

            }

            if (!string.IsNullOrEmpty(buttonText))
            {
                var startButton = new Button
                {
                    Padding = new Thickness(5),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Content = new Label { Text = buttonText, },
                    Enabled = isButtonActive,
                };
                if (isButtonActive)
                {
                    startButton.Click += (s, a) =>
                    {
                        if (selectedDungeon.DungeonState == DungeonState.Completed && selectedDungeon.Mission != null)
                        {
                            var party = selectedDungeon.Mission.Party;
                            foreach (var hero in party.ToList())
                            {
                                WorldInstance.Guild.Hire(hero);
                                selectedDungeon.Mission.RemoveParty(hero);
                            }
                            selectedDungeon.Mission = null;
                            var dungeon = WorldInstance.Guild.OwnedDungeons.Find(e => e == selectedDungeon);
                            WorldInstance.Guild.RemoveDungeon(dungeon);
                            selectedDungeon = null;
                            Build();
                            return;
                        }
                        else if (selectedDungeon.DungeonState == DungeonState.NotStarted)
                        {
                            if (selectedDungeon.Mission.Party.Count > 0)
                            {
                                selectedDungeon.Mission.Start();
                                Build();
                            }
                        }
                    };
                }
                _partyPanel.Widgets.Add(startButton);
            }
        }

        foreach (var guildDungeon in WorldInstance.Guild.OwnedDungeons)
        {
            var card = CreateDungeonCard(guildDungeon, true);
            _rosterDungeonPanel.Widgets.Add(card);
        }

        foreach (var dungeon in WorldInstance.Guild.AvailableDungeons)
        {
            var card = CreateDungeonCard(dungeon, false);
            _availableDungeonPanel.Widgets.Add(card);
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
        infoStack.Widgets.Add(new Label { Text = $"Element: {hero.Element.Type}" });


        var statsLabel = new Label { Text = $"HP: {hero.HP}/{hero.MaxHP} | Energy: {hero.Energy}/{hero.MaxEnergy} | Mana: {hero.Mana}/{hero.MaxMana}" };
        if (hero.HP < hero.MaxHP * 0.3f) statsLabel.TextColor = Color.Red;
        infoStack.Widgets.Add(statsLabel);

        infoStack.Widgets.Add(new Label { Text = $"STR: {hero.Strength} | DEX: {hero.Dexterity} | CON: {hero.Constitution} | WIS: {hero.Wisdom} | CHAR: {hero.Charisma}" });
        infoStack.Widgets.Add(new Label { Text = $"Phys Dmg: {hero.PhysicalDamage} | Magic Pow: {hero.MagicPower} | Armour: {hero.Armour}" });

        Grid.SetColumn(infoStack, 0);
        grid.Widgets.Add(infoStack);

        var buttonStack = new VerticalStackPanel { Spacing = 5, VerticalAlignment = VerticalAlignment.Center };
        var buttonText = "";
        float hireMultiplier = 1f;
        switch (cardType)
        {
            case CardType.Applicant:
                buttonText = $"Hire {(int)(hero.Wage * hireMultiplier)}g";
                break;
            case CardType.GuildHero:
                if (CurrentUIState == UIState.MissionSelectParty)
                {
                    buttonText = "Add";
                }
                else
                {
                    buttonText = "Fire";
                }
                break;
            case CardType.PartyHero:
                buttonText = "Remove";
                break;

        }

        bool isButtonActive = true;
        if (CurrentUIState == UIState.MissionSelectParty)
        {
            if (selectedDungeon != null)
            {
                if (selectedDungeon.Mission != null)
                {
                    isButtonActive = true;
                }
                else
                {
                    isButtonActive = false;
                }
            }
        }
        var actionButton = new Button
        {
            Padding = new Thickness(5),
            Content = new Label
            {
                Text = buttonText,
                HorizontalAlignment = HorizontalAlignment.Center,
            },
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
            bool needsRest = hero.HP < hero.MaxHP ||
                             hero.Mana < hero.MaxMana ||
                             hero.Energy < hero.MaxEnergy ||
                             hero.Stressed ||
                             hero.CurrentStress > 0;

            if (cardType == CardType.GuildHero || cardType == CardType.Applicant)
            {
                var restButton = new Button
                {
                    Padding = new Thickness(5),
                    Background = new SolidBrush(new Color(0, 100, 0, 200)), // Fundo Verde Escuro
                    Border = new SolidBrush(Color.Green),
                    BorderThickness = new Thickness(1),
                    Enabled = needsRest,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    Content = new Label
                    {
                        Text = needsRest ? $"Rest" : "Rested",
                        TextColor = needsRest ? Color.Red : Color.LightGreen,
                        HorizontalAlignment = HorizontalAlignment.Center
                    }
                };

                restButton.Click += (s, a) =>
                {
                    hero.CombatRest(null);
                    RefreshInfo();
                };

                buttonStack.Widgets.Add(restButton);
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

        var grid = new Grid { ColumnSpacing = 10 };
        grid.ColumnsProportions.Add(new Proportion(ProportionType.Fill));
        grid.ColumnsProportions.Add(new Proportion(ProportionType.Auto));
        grid.RowsProportions.Add(new Proportion(ProportionType.Auto));

        var dungeonInfo = new VerticalStackPanel();
        dungeonInfo.Widgets.Add(new Label { Text = $"{dungeon.Biome.Type} (Lvl {dungeon.Level})", TextColor = Color.Cyan });
        if (IsOwned)
        {
            var resourcesList = dungeon.DungeonResources.Resources.Select(r => $"{r.Type}: {r.Quantity}");
            int totalValue = dungeon.DungeonResources.Resources.Sum(r => r.Price * r.Quantity);
            dungeonInfo.Widgets.Add(new Label {Text = $"Rooms: {dungeon.Rooms.Count}"});
            dungeonInfo.Widgets.Add(new Label { Text = string.Join(" | ", resourcesList), Wrap = true });
            dungeonInfo.Widgets.Add(new Label { Text = $"Est. Value: {totalValue}g", TextColor = Color.Green });

        }
        else
        {
            int totalValue = dungeon.DungeonResources.Resources.Sum(r => r.Price * r.Quantity);
            int profitMargin = totalValue - dungeon.Price;

            dungeonInfo.Widgets.Add(new Label { Text = $"Cost: {dungeon.Price}g", TextColor = Color.Red });
            dungeonInfo.Widgets.Add(new Label { Text = $"Est. Value: {totalValue}g", TextColor = Color.Green });
            dungeonInfo.Widgets.Add(new Label { Text = $"Potential Profit: {profitMargin}g", TextColor = Color.Gold });
        }
        var elementsList = dungeon.Biome.BiomeCurrentElements.Select(e => $"{e.ToString()}");
        var elementsText = string.Join(" , ", elementsList);
        dungeonInfo.Widgets.Add(new Label { Text = "Elements: " + elementsText });
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
}