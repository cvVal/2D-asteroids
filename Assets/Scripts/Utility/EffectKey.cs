namespace Utility
{
    /// <summary>
    /// Enum defining all VFX and SFX types in the game.
    /// Easily extensible for new effects or sounds.
    /// </summary>
    public enum EffectKey
    {
        None = 0,
        
        // Music
        MainMenuMusic,
        GameplayMusic,
        
        // Explosion SFX
        PlayerExplosion,
        AsteroidExplosion,
        EnemyExplosion,
        
        // Game Event Sounds
        GameOver,
        Win,
        
        // Additional SFX
        PlayerShoot,
        EnemyShoot
    }
}

