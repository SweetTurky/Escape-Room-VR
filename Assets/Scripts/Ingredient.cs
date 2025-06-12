using UnityEngine;

public enum IngredientType { DriedWorm, FirebloomPetals, DragonsTooth /*…etc…*/ }

public class Ingredient : MonoBehaviour
{
    [Tooltip("Select which ingredient this is")]
    public IngredientType ingredientType;
}