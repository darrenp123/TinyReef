
// Interface meant to be used on every class that can be consumed by fish
// ex: coral and fish
public interface IFood
{
    // methods to be implemented for every class that use this interface
    void Consume();

    string GetFoodName();

    float GetNutricionValue();
}
