using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Polygoat.Haptics;
using System.ComponentModel;
using System.IO;

public class LightHapticEventMenu : MonoBehaviour
{
    public Button btnDelete;

    public Button btnSettings;
    public GameObject settingsPanel;
    public Toggle tglEnable;

    public ToggleGroup mappingGroup;
    public Toggle mappingInstantaneous;
    public Toggle mappingContinuousPassive;
    public Toggle mappingContinuousActive;
    public Toggle mappingPositional;
    public MappingLabel lblMapping;
    public Text lblDuration;

    public Slider expressiveness;
    public Slider randomness;
    public Slider intensity;
    public Slider modulation;

    public int barWidth = 20;
    public int spaceWidth = 50;

    public Color waveformColor = new Color(0.5f, 0.1f, 0.5f);
    public Color bgColor = new Color(0.5f, 0.1f, 0.5f);

    public Image imgWaveform;

    public bool saveTextures = true;

    private Texture2D waveformTexture;

    // index of the event in the grab/touch event list
    private int eventIndex;
    public HapticInteractable HapticInteractable { get; private set; }
    public HapticData HapticData { get; private set; }
    public HapticEventData CurrentHapticEventData
    {
        get
        {
            List<HapticEventData> eventList = GetCurrentHapticEventDataList();
            if (eventList != null && eventIndex < eventList.Count)
                return eventList[eventIndex];
            else
                return null;
        }
    }


    private Texture2D tex;
    private Image imgWave;

    public virtual List<HapticEventData> GetCurrentHapticEventDataList()
    {
        return null;
    }



    private void OnEnable()
    {

        //        Debug.Log("Enabling LightHapticEventMenu " + CurrentHapticEventData);
        Canvas canvas = GetComponent<Canvas>();
        canvas.worldCamera = GameObject.FindGameObjectsWithTag("MainCamera")[0].GetComponent<Camera>();

        // primary buttons
        btnDelete.onClick.AddListener(HandleDeleteButtonClick);
        btnSettings.onClick.AddListener(HandleSettingsButtonClick);

        // enable toggle
        tglEnable.onValueChanged.AddListener(HandleEnabledValueChanged);

        // mapping toggles
        mappingInstantaneous.onValueChanged.AddListener(HandleMappingInstantaneousChange);
        mappingContinuousPassive.onValueChanged.AddListener(HandleMappingContinuousPassiveChange);
        mappingContinuousActive.onValueChanged.AddListener(HandleMappingContinuousActiveChange);
        mappingPositional.onValueChanged.AddListener(HandleMappingPositionalChange);

        // sliders
        expressiveness.onValueChanged.AddListener(HandleExpressivenessChange);
        randomness.onValueChanged.AddListener(HandleRandomnessChange);
        intensity.onValueChanged.AddListener(HandleIntensityChange);
        modulation.onValueChanged.AddListener(HandleModulationChange);
    }

    private void OnDisable()
    {
        btnDelete.onClick.RemoveAllListeners();
        btnSettings.onClick.RemoveAllListeners();

        mappingInstantaneous.onValueChanged.RemoveAllListeners();
        mappingContinuousPassive.onValueChanged.RemoveAllListeners();
        mappingContinuousActive.onValueChanged.RemoveAllListeners();
        mappingPositional.onValueChanged.RemoveAllListeners();

        expressiveness.onValueChanged.RemoveAllListeners();
        randomness.onValueChanged.RemoveAllListeners();
        intensity.onValueChanged.RemoveAllListeners();
        modulation.onValueChanged.RemoveAllListeners();
    }

    public void Initialize(HapticInteractable hapticInteractable, HapticData hapticData, int eventIndex)
    {
        HapticInteractable = hapticInteractable;
        HapticData = hapticData;
        //Debug.Log("menu initialize - grab list " + HapticData.grabEventData.Count);
        this.eventIndex = eventIndex;
        UpdateForm();
    }

    public void UpdateIndex(int index)
    {
        this.eventIndex = index;
    }


    private void HandleDeleteButtonClick()
    {
        Destroy(this.gameObject);
        RemoveEventData(eventIndex);
        HapticEditor.Instance.SaveHapticData();
        // Debug.Log("trying to remove");
    }

    private void HandleSettingsButtonClick()
    {
        settingsPanel.SetActive(!settingsPanel.activeSelf);
    }


    private void HandleEnabledValueChanged(bool enabled)
    {
        // Debug.Log("HandleEnabledValueChanged");
        CurrentHapticEventData.enabled = enabled;
    }


    private void HandleMappingInstantaneousChange(bool enabled)
    {
        if (enabled)
        {
            // ugly fix to strange toggle group behavior
            mappingContinuousPassive.SetIsOnWithoutNotify(false);
            mappingContinuousActive.SetIsOnWithoutNotify(false);
            mappingPositional.SetIsOnWithoutNotify(false);
            lblMapping.SetType(HapticType.Instantaneous);
            if (CurrentHapticEventData != null)
                CurrentHapticEventData.hapticType = HapticType.Instantaneous;
        }
    }

    private void HandleMappingContinuousPassiveChange(bool enabled)
    {
        if (enabled)
        {
            // ugly fix to strange toggle group behavior
            mappingInstantaneous.SetIsOnWithoutNotify(false);
            mappingContinuousActive.SetIsOnWithoutNotify(false);
            mappingPositional.SetIsOnWithoutNotify(false);
            lblMapping.SetType(HapticType.ContinuousPassive);
            if (CurrentHapticEventData != null)
                CurrentHapticEventData.hapticType = HapticType.ContinuousPassive;
        }
    }

    private void HandleMappingContinuousActiveChange(bool enabled)
    {
        if (enabled)
        {
            // ugly fix to strange toggle group behavior
            mappingInstantaneous.SetIsOnWithoutNotify(false);
            mappingContinuousPassive.SetIsOnWithoutNotify(false);
            mappingPositional.SetIsOnWithoutNotify(false);
            lblMapping.SetType(HapticType.ContinuousActive);
            if (CurrentHapticEventData != null)
                CurrentHapticEventData.hapticType = HapticType.ContinuousActive;
        }
    }


    private void HandleMappingPositionalChange(bool enabled)
    {
        if (enabled)
        {
            // ugly fix to strange toggle group behavior
            mappingInstantaneous.SetIsOnWithoutNotify(false);
            mappingContinuousPassive.SetIsOnWithoutNotify(false);
            mappingContinuousActive.SetIsOnWithoutNotify(false);
            lblMapping.SetType(HapticType.Positional);
            if (CurrentHapticEventData != null)
                CurrentHapticEventData.hapticType = HapticType.Positional;
        }
    }




    protected virtual void RemoveEventData(int eventIndex)
    {

    }


    private void HandleExpressivenessChange(float value)
    {
        CurrentHapticEventData.expressiveness = value;
    }

    private void HandleRandomnessChange(float value)
    {
        CurrentHapticEventData.randomness = value;
    }

    private void HandleIntensityChange(float value)
    {
        CurrentHapticEventData.intensity = value;
    }

    private void HandleModulationChange(float value)
    {
        CurrentHapticEventData.modulation = value;
    }




    protected virtual void UpdateForm()
    {
        //Debug.LogWarning("UpdateForm");

        HapticEventData currentEvent = CurrentHapticEventData;
        if (currentEvent == null)
        {
            Debug.Log("currentEvent is null (destroying layer)");
            Destroy(this.gameObject);
            return;
        }
        else
        {
            //Debug.Log("currentEvent is NOT null");

            tglEnable.SetIsOnWithoutNotify(currentEvent.enabled);

            //update controls
            mappingInstantaneous.SetIsOnWithoutNotify(currentEvent.hapticType == HapticType.Instantaneous);
            mappingContinuousPassive.SetIsOnWithoutNotify(currentEvent.hapticType == HapticType.ContinuousPassive);
            mappingContinuousActive.SetIsOnWithoutNotify(currentEvent.hapticType == HapticType.ContinuousActive);

            // Fucking dirty hack for positional button
            if (currentEvent.audioClipScaledSubPath == null || currentEvent.audioClipScaledSubPath.Length == 0)
            {
                mappingPositional.gameObject.SetActive(false);
            }
            else
            {
                mappingPositional.SetIsOnWithoutNotify(currentEvent.hapticType == HapticType.Positional);
            }

            lblMapping.SetType(currentEvent.hapticType);

            //update duration
            float duration = CurrentHapticEventData.HapticClip.GetDuration();
            lblDuration.text = duration.ToString("0.00") + "s";

            //update waveform
            if (currentEvent.AudioClip != null)
            {
                StartCoroutine(buildWaveformSpectrum(currentEvent.AudioClip, 1f, (int)imgWaveform.rectTransform.rect.width, (int)imgWaveform.rectTransform.rect.height, waveformColor, bgColor,
                    sprite =>
                    {
                        if (sprite == null)
                        {
                            // Debug.LogError("tex == null");
                            return;
                        }
                        // Debug.Log("saveTextures: " + saveTextures);
                        //if (saveTextures)
                        //{
                            string name = Path.GetFileNameWithoutExtension(CurrentHapticEventData.AudioClipFullPath);
                            // Debug.Log("name: " + name);
                            // Path.Combine(Application.streamingAssetsPath, audioClipSubPath);
                            SaveSpriteToFile(sprite, Path.GetFileNameWithoutExtension(name));
                        //}

                        //Debug.Log("tex != null");
                        imgWaveform.sprite = sprite;
                    }));
                // StartCoroutine(DrawWaveformCoroutine(currentEvent));
                // imgWave = imgWaveform;
                // StartCoroutine(DrawWaveformCoroutine(currentEvent.AudioClip, (int)imgWaveform.rectTransform.rect.width, (int)imgWaveform.rectTransform.rect.height, waveformColor, bgColor));

                //waveformTexture = buildWaveformSpectrum(currentEvent.AudioClip, 1f, (int)imgWaveform.rectTransform.rect.width, (int)imgWaveform.rectTransform.rect.height, waveformColor, bgColor);
                //imgWaveform.sprite = Sprite.Create(waveformTexture, new Rect(0f, 0f, waveformTexture.width, waveformTexture.height), new Vector2(0.5f, 0.5f));
            }
            else
            {
                imgWaveform.sprite = null;
            }

            // update sliders
            expressiveness.value = currentEvent.expressiveness;
            randomness.value = currentEvent.randomness;
            intensity.value = currentEvent.intensity;
            modulation.value = currentEvent.modulation;
        }
    }


    private void SaveSpriteToFile(Sprite sprite, string outfile)
    {
        Texture2D texture = textureFromSprite(sprite);
        //byte[] bytes = sprite.texture.EncodeToPNG();
        byte[] bytes = texture.EncodeToPNG();
        var dirPath = Application.dataPath + "/savedTextures/";
        if (!Directory.Exists(dirPath))
        {
            Directory.CreateDirectory(dirPath);
        }
        // Debug.Log("Writing sprite to: " + outfile + ".png");
        File.WriteAllBytes(dirPath + outfile + ".png", bytes);

    }

    private Texture2D textureFromSprite(Sprite sprite)
    {
        if (sprite.rect.width != sprite.texture.width)
        {
            Texture2D newText = new Texture2D((int)sprite.rect.width, (int)sprite.rect.height);
            Color[] newColors = sprite.texture.GetPixels((int)sprite.textureRect.x,
                                                         (int)sprite.textureRect.y,
                                                         (int)sprite.textureRect.width,
                                                         (int)sprite.textureRect.height);
            newText.SetPixels(newColors);
            newText.Apply();
            return newText;
        }
        else
            return sprite.texture;
    }


    // private Texture2D buildWaveformSpectrum(AudioClip audio, float saturation, int width, int height, Color waveCol, Color bgCol)
    private IEnumerator buildWaveformSpectrum(AudioClip audio, float saturation, int width, int height, Color waveCol, Color bgCol, System.Action<Sprite> callback)
    {
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        //Debug.Log("Texture width " + width + ", height " + height);
        int barCount = (int)Mathf.Floor((width - spaceWidth) / (barWidth + spaceWidth));
        int packSize = (audio.samples / barCount);

        float[] waveform = new float[width];
        float[] samples = new float[audio.samples];
        audio.GetData(samples, 0);
        int s = 0;

        AudioCalibration calibration = GameObject.FindObjectOfType<AudioCalibration>();

        // Debug.Log("audio.samples:" + audio.samples);
        for (int i = 0; i < audio.samples; i += packSize)
        {
            // Debug.Log("i:" + i);
            float avg = 0;
            int count = 0;
            for (int z = 0; z < packSize && i + z < audio.samples; z++)
            {
                // Debug.Log("z:" + z);
                avg += Mathf.Abs(samples[i + z]);
                count++;
            }

            //Debug.Log("avg:" + avg);
            //Debug.Log("count:" + count);

            // waveform[s] = Mathf.Abs(samples[i]);
            waveform[s] = calibration.MapAmplitudeToCalibration(avg / (float)count);
            s++;
            //yield return null;
        }

        Color[] pixels = new Color[width*height];
        for (int x = 0; x < width; x++)
        { 
            for (int y = 0; y < height; y++)
            {
                pixels[x + width * y] = bgCol;
                // tex.SetPixel(x, y, bgCol);
            }
        }

        

        //yield return null;

        float maxHeight = height * 0.75f;
        float middle = height / 2;
        for (int x = 0; x < barCount; x++)
        {
            int posX = (x * (barWidth + spaceWidth)) + spaceWidth;

            for (int inc = 0; inc <= barWidth; inc++)
            {
                for (int y = 0; y <= (waveform[x] * (maxHeight / 2)); y++)
                {
                    // tex.SetPixel(posX + inc, (int)(middle + y), waveCol);
                    // tex.SetPixel(posX + inc, (int)(middle - y), waveCol);
                    pixels[posX + inc + (width * (int)(middle + y))] = waveCol;
                    pixels[posX + inc + (width * (int)(middle - y))] = waveCol;
                }
            }

            int max = (int)(waveform[x] * (maxHeight / 2));
            DrawCircle(width, waveformColor, (int)(posX + (barWidth / 2)), (int)(middle + max), barWidth / 2, ref pixels);
            DrawCircle(width, waveformColor, (int)(posX + (barWidth / 2)), (int)(middle - max), barWidth / 2, ref pixels);
            //yield return null;
        }

        tex.SetPixels(pixels);
        //yield return null;
        tex.Apply();

        //yield return null;

        if (callback != null) {
            //callback.Invoke(tex);
            callback.Invoke(Sprite.Create(tex, new Rect(0f, 0f, tex.width, tex.height), new Vector2(0.5f, 0.5f)));
        }

        yield return null;
        // return tex;
    }

    

    /* private void DrawCircle(Texture2D tex, Color color, int x, int y, int radius = 3)
     {
         float rSquared = radius * radius;

         for (int u = x - radius; u < x + radius + 1; u++)
             for (int v = y - radius; v < y + radius + 1; v++)
                 if ((x - u) * (x - u) + (y - v) * (y - v) < rSquared)
                     tex.SetPixel(u, v, color);

         // return tex;
     }*/

    private void DrawCircle(int width, Color color, int x, int y, int radius, ref Color[] pixels)
    {
        float rSquared = radius * radius;

        for (int u = x - radius; u < x + radius + 1; u++)
            for (int v = y - radius; v < y + radius + 1; v++)
                if ((x - u) * (x - u) + (y - v) * (y - v) < rSquared)
                    pixels[u + (width * v)] = color;
                    // tex.SetPixel(u, v, color);

        // return tex;
    }

}
