using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnitySensors;

public class LidarImageVisualizer : MonoBehaviour
{
    public VelodyneSensor lidarSensor; // 유니티에서 구현된 라이다 센서
    public RawImage displayImage; // 이미지를 표시할 RawImage 컴포넌트
    private Texture2D scanTexture;
    public int imageSize = 300; // 텍스처 크기
    public int pointSize = 3;

    void Start()
    {
        // 텍스처 초기화
        scanTexture = new Texture2D(imageSize, imageSize, TextureFormat.RGB24, false);
        scanTexture.filterMode = FilterMode.Point; // 텍스처 필터링 없음

        // 텍스처를 초기 흰색으로 채우기
        Color[] fillColor = new Color[imageSize * imageSize];
        for (int i = 0; i < fillColor.Length; i++)
        {
            fillColor[i] = Color.white;
        }

        scanTexture.SetPixels(fillColor);
        scanTexture.Apply();

        // RawImage 컴포넌트에 텍스처 설정
        displayImage.texture = scanTexture; // 수정된 부분
    }
    
    void DrawCircle(Texture2D texture, int centerX, int centerY, int radius, Color color)
    {
        int x = radius;
        int y = 0;
        int radiusError = 1 - x;

        while (x >= y)
        {
            texture.SetPixel(centerX + x, centerY + y, color);
            texture.SetPixel(centerX + y, centerY + x, color);
            texture.SetPixel(centerX - y, centerY + x, color);
            texture.SetPixel(centerX - x, centerY + y, color);
            texture.SetPixel(centerX - x, centerY - y, color);
            texture.SetPixel(centerX - y, centerY - x, color);
            texture.SetPixel(centerX + y, centerY - x, color);
            texture.SetPixel(centerX + x, centerY - y, color);

            y++;

            if (radiusError < 0)
            {
                radiusError += 2 * y + 1;
            }
            else
            {
                x--;
                radiusError += 2 * (y - x + 1);
            }
        }
    }
    
    void Update()
    {
        // 텍스처를 클리어하고 새 데이터로 업데이트
        ClearTexture();
        var ranges = lidarSensor.GetDistances(); // 라이다 거리 데이터 가져오기

        for (int i = 0; i < ranges.Length; i++)
        {
            float range = ranges[i];
            float angle = i * 2 * Mathf.PI / ranges.Length; // 라이다 데이터의 각도 계산

            // 극좌표계에서 직교좌표계로 변환
            int x = Mathf.RoundToInt(imageSize / 2 + (range * Mathf.Cos(angle) * imageSize / 2));
            int y = Mathf.RoundToInt(imageSize / 2 + (range * Mathf.Sin(angle) * imageSize / 2));

            // 텍스처에 데이터 포인트 그리기
            DrawCircle(scanTexture, x, y, pointSize, Color.green); // 수정된 부분: 포인트 크기를 조절할 수 있는 pointSize 변수 사용
        }

        // 텍스처 변경 적용
        scanTexture.Apply();
    }
    
    void ClearTexture()
    {
        // 텍스처를 흰색으로 초기화
        Color[] clearColors = new Color[imageSize * imageSize];
        for (int i = 0; i < clearColors.Length; i++)
        {
            clearColors[i] = Color.white;
        }

        scanTexture.SetPixels(clearColors);
    }

}
