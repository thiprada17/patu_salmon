using UnityEngine;

public class Grinder : MonoBehaviour
{
    private Portfilter currentPortfilter;

    public void AttachPortfilter(Portfilter portfilter)
    {
        currentPortfilter = portfilter;

        Debug.Log("Portfilter attached to Grinder!");
    }

    private void OnMouseDown()
    {
        if (currentPortfilter == null)
        {
            Debug.Log("No Portafilter!");
            return;
        }

        Grind(currentPortfilter);
    }

    public void Grind(Portfilter portfilter)
    {
        if (portfilter.hasCoffeeGround)
        {
            Debug.Log("Already has coffee ground!");
            return;
        }

        portfilter.AddCoffeeGround();

        Debug.Log("Grinding coffee...");
    }
}