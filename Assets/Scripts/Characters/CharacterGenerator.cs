using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterGenerator : MonoBehaviour {
    public static CharacterGenerator Instance { get; private set; }
    
    public List<Sprite> HeadSprites;
    public List<Sprite> HairSprites;
    public List<Sprite> EyesSprites;
    public List<Sprite> NoseSprites;
    public List<Sprite> MouthSprites;
    public List<Sprite> BodySprites;
    public List<Sprite> LegsSprites;
    
    protected void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
        } else {
            Instance = this;
            Service.CharacterGenerator = this;
        }
        
        GenerateCharacters();
    }

    private List<CharacterAttributes> _charactersAttributes;

    public CharacterAttributes Generate() {
        CharacterAttributes attributes = new CharacterAttributes() {
            HeadType = (HeadSprites != null) ? Random.Range(0, HeadSprites.Count) : 0,
            HairType = (HairSprites != null) ? Random.Range(0, HairSprites.Count) : 0,
            EyesType = (EyesSprites != null) ? Random.Range(0, EyesSprites.Count) : 0,
            NoseType = (NoseSprites != null) ? Random.Range(0, NoseSprites.Count) : 0,
            MouthType = (MouthSprites != null) ? Random.Range(0, MouthSprites.Count) : 0,
            BodyType = (BodySprites != null) ? Random.Range(0, BodySprites.Count) : 0,
            LegsType = (LegsSprites != null) ? Random.Range(0, LegsSprites.Count) : 0,
        };

        return attributes;
    }

    private void GenerateCharacters() {
        _charactersAttributes = new List<CharacterAttributes>();
        for (int i = 0; i < ConfigManager.NumberOfCharactersToGenerate; i++) {
            CharacterAttributes attributes = new CharacterAttributes() {
                HeadType = (HeadSprites != null) ? Random.Range(0, HeadSprites.Count) : 0,
                HairType = (HairSprites != null) ? Random.Range(0, HairSprites.Count) : 0,
                EyesType = (EyesSprites != null) ? Random.Range(0, EyesSprites.Count) : 0,
                NoseType = (NoseSprites != null) ? Random.Range(0, NoseSprites.Count) : 0,
                MouthType = (MouthSprites != null) ? Random.Range(0, MouthSprites.Count) : 0,
                BodyType = (BodySprites != null) ? Random.Range(0, BodySprites.Count) : 0,
                LegsType = (LegsSprites != null) ? Random.Range(0, LegsSprites.Count) : 0,
            };

            _charactersAttributes.Add(attributes);
        }
    }

    public CharacterAttributes AttributesForCharacter(Character character) {
        if (character.Index >= 0 && character.Index < _charactersAttributes.Count) {
            return _charactersAttributes[character.Index];
        }
        
        return new CharacterAttributes();
    }

    public CharacterAttributes AttributesForEmoteType(Emote.EmoteSubType emoteType) {
        int index = ((int) emoteType) - ((int) Emote.EmoteSubType.CharacterHeadshot_1);
        if (index > 0 && index < _charactersAttributes.Count) {
            return _charactersAttributes[index];
        }
        
        return new CharacterAttributes();
    }
}
