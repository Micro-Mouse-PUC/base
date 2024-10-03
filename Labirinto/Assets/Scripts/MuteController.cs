using UnityEngine;
using UnityEngine.UI;

public class ToggleButtonSprite : MonoBehaviour
{
    public Sprite spriteOn;  // Sprite para o estado "ligado"
    public Sprite spriteOff; // Sprite para o estado "desligado"
    
    private Image buttonImage;  // Referência para o componente Image do botão
    private bool isOn = false;  // Estado inicial do botão (desligado)

    void Start()
    {
        // Pega o componente Image do botão
        buttonImage = GetComponent<Image>();

        // Define o sprite inicial como "off"
        buttonImage.sprite = spriteOff;
    }

    public void ToggleSprite()
    {
        // Alterna o estado entre "ligado" e "desligado"
        isOn = !isOn;

        // Altera o sprite com base no estado atual
        if (isOn)
        {
            buttonImage.sprite = spriteOn;
        }
        else
        {
            buttonImage.sprite = spriteOff;
        }
    }
}
