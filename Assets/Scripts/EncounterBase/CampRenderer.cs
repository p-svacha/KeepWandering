using UnityEngine;

public class CampRenderer : MonoBehaviour
{
    public GameObject Tent;
    public SpriteRenderer TentSpot;
    public SpriteRenderer Bedroll;
    public SpriteRenderer BedrollSpot;
    public SpriteRenderer FireSpot;
    public SpriteRenderer FireFlame;
    public SpriteRenderer Trap1;
    public SpriteRenderer Trap2;
    public SpriteRenderer Trap3;

    private Camp Camp => Camp.Instance;

    /// <summary>
    /// Refreshes all sprites according to the current state of the camp.
    /// </summary>
    public void Refresh()
    {
        // Tent
        Tent.SetActive(Camp.HasTent);
        TentSpot.gameObject.SetActive(true);

        // Bedroll
        Bedroll.gameObject.SetActive(Camp.HasBedroll);
        BedrollSpot.gameObject.SetActive(true);

        // Fire
        FireSpot.gameObject.SetActive(true);
        FireFlame.gameObject.SetActive(Camp.HasFire);

        if (Camp.HasFire) FireSpot.sprite = ResourceManager.LoadSprite("Camp/Camp_FireSpotWithWood");
        else FireSpot.sprite = ResourceManager.LoadSprite("Camp/Camp_FireSpot");

        // Traps
        RefreshTrap(Trap1, Camp.Trap1 != null);
        RefreshTrap(Trap2, Camp.Trap2 != null);
        RefreshTrap(Trap3, Camp.Trap3 != null);
    }

    private void RefreshTrap(SpriteRenderer sprite, bool hasTrap)
    {
        sprite.gameObject.SetActive(true);
        if (hasTrap) sprite.sprite = ResourceManager.LoadSprite("Camp/Camp_Trap");
        else sprite.sprite = ResourceManager.LoadSprite("Camp/Camp_TrapSpot");
    }
}
