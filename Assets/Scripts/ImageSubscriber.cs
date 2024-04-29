using UnityEngine;
using UnityEngine.UI;
using RosMessageTypes.Sensor;
using Unity.Robotics.ROSTCPConnector;

public class ImageSubscriber : MonoBehaviour
{
    private Texture2D texture; // Texture2D 객체
    public RawImage displayImage; // UI에서 이미지를 표시할 RawImage 컴포넌트
    private byte[] imageData;
    void Start()
    {
        // ROS 토픽 구독 설정
        ROSConnection.GetOrCreateInstance().Subscribe<ImageMsg>("/scan_image", ImageCallback);
        texture = new Texture2D(2, 2);
    }

    private void ImageCallback(ImageMsg imageMsg)
    {
        imageData = imageMsg.data;
        texture.LoadImage(imageData);
        texture.Apply();
    }
}