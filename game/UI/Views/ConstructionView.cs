using Myra.Graphics2D.UI;
using Microsoft.Xna.Framework;
using Myra.Graphics2D.Brushes;
using Myra.Graphics2D;
using System.Linq;
using System;
using System.ComponentModel;

public class ConstructionView : ISubView
{
    private Grid _mainGrid;
    private Grid _itemsGrid;
    private Label _descriptionLabel;

    private UIController _controller;
    private BuildCategory _currentCategory = BuildCategory.Structure;

    public Widget GetRootPanel()
    {
        return _mainGrid;
    }

    public void Refresh()
    {
        if (_controller != null && _mainGrid != null)
        {
            RefreshItemsList(_currentCategory, _controller);
        }
    }

    public Widget Build(UIController controller)
    {
        _controller = controller;
        _mainGrid = new Grid
        {
            RowSpacing = 5,
            Padding = new Thickness(5)
        };

        _mainGrid.RowsProportions.Add(new Proportion(ProportionType.Auto));
        _mainGrid.RowsProportions.Add(new Proportion(ProportionType.Fill));
        _mainGrid.RowsProportions.Add(new Proportion(ProportionType.Pixels, 40));

        var categoryContainer = new HorizontalStackPanel { Spacing = 5 };

        foreach (BuildCategory category in Enum.GetValues<BuildCategory>())
        {
            var btn = new Button
            {
                Content = new Label { Text = category.ToString() },
                Padding = new Thickness(5),
            };

            btn.Click += (s, a) =>
            {
                _currentCategory = category;
                RefreshItemsList(category, controller);

            };
            categoryContainer.Widgets.Add(btn);
        }

        var catScroll = new ScrollViewer { Content = categoryContainer };
        _mainGrid.Widgets.Add(catScroll);

        _itemsGrid = new Grid
        {
            ColumnSpacing = 5,
            RowSpacing = 5,
            Padding = new Thickness(5)
        };

        for (int i = 0; i < 4; i++)
        {
            _itemsGrid.ColumnsProportions.Add(new Proportion(ProportionType.Part));
        }

        var itemScroll = new ScrollViewer { Content = _itemsGrid };
        Grid.SetRow(itemScroll, 1);
        _mainGrid.Widgets.Add(itemScroll);

        _descriptionLabel = new Label { Text = "Select a category...", TextColor = Color.Gray };
        Grid.SetRow(_descriptionLabel, 2);
        _mainGrid.Widgets.Add(_descriptionLabel);

        RefreshItemsList(BuildCategory.Structure, controller);

        return _mainGrid;
    }

    private void RefreshItemsList(BuildCategory category, UIController controller)
    {
        if (_itemsGrid == null) return;

        _itemsGrid.Widgets.Clear();
        _itemsGrid.RowsProportions.Clear();

        var items = BuildDatabase.GetByCategory(category);
        int columns = 4;
        int row = 0;
        int col = 0;

        _itemsGrid.RowsProportions.Add(new Proportion(ProportionType.Auto));

        foreach (var item in items)
        {
            var itemButton = CreateItemButton(item, controller);

            Grid.SetColumn(itemButton, col);
            Grid.SetRow(itemButton, row);
            _itemsGrid.Widgets.Add(itemButton);

            col++;
            if (col >= columns)
            {
                col = 0;
                row++;
                _itemsGrid.RowsProportions.Add(new Proportion(ProportionType.Auto));
            }
        }
    }

    private Button CreateItemButton(Buildable item, UIController controller)
    {
        var btn = new Button
        {
            Width = 60,
            Height = 60,
            Background = new SolidBrush(item.PlaceholderColor),
            BorderThickness = new Myra.Graphics2D.Thickness(2)
        };

        var contentStack = new VerticalStackPanel();
        contentStack.Widgets.Add(new Label { Text = item.Name, Wrap = true });
        contentStack.Widgets.Add(new Label { Text = $"{item.Size.X}x{item.Size.Y}", TextColor = Color.Gray });
        var playerGold = UIController.WorldInstance?.Guild?.Gold ?? 0;
        var costColor = (playerGold >= item.GoldCost) ? Color.Gold : Color.Red;

        contentStack.Widgets.Add(new Label { Text = $"{item.GoldCost}g", TextColor = costColor });

        btn.Content = contentStack;

        btn.MouseEntered += (s, a) =>
        {
            _descriptionLabel.Text = $"{item.Name}: {item.Description} (Cost: {item.GoldCost})";
        };

        btn.Click += (s, a) =>
        {

            if (UIController.WorldInstance.Guild.Gold >= item.GoldCost)
            {
                UIController.WorldInstance.EnterBuildMode(item);
                Console.WriteLine(item);

            }
            else
            {
                Console.WriteLine("Not enough gold!");
            }
        };
        return btn;
    }
}