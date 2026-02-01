using Myra.Graphics2D.UI;

public interface ISubView
{
    Widget GetRootPanel();

    void Refresh();

    Widget Build(UIController controller);
}