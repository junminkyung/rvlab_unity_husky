#!/usr/bin/env python3
# -*- coding: utf-8 -*-
import rospy
from sensor_msgs.msg import LaserScan
from nav_msgs.msg import Odometry
from geometry_msgs.msg import Twist
from geometry_msgs.msg import PoseStamped
from geometry_msgs.msg import PointStamped
from visualization_msgs.msg import Marker
from geometry_msgs.msg import Point
import numpy as np
import onnxruntime as ort
import scipy.signal
from scipy.ndimage import gaussian_filter

class ONNXInference:
    def __init__(self, model_path):
        self.model_path = model_path
        self.session = ort.InferenceSession(model_path)
        self.input_shape = (1, 905)  # 예를 들어 모델 입력이 905차원이라고 가정
        self.scan_data = np.zeros((897,), dtype=np.float32)  # LaserScan 데이터
        self.odom_data = np.zeros((3,), dtype=np.float32)    # Odometry 위치 데이터
        self.twist_data = np.zeros((2,), dtype=np.float32)   # Twist 선속도 데이터
        self.goal_data = np.zeros((3,), dtype=np.float32)    # 목표 지점 데이터
        self.twist_publisher = rospy.Publisher('onnx/cmd_vel', Twist, queue_size=10)
        # self.marker_publisher = rospy.Publisher('/visualization_marker', Marker, queue_size=10)
        # self.marker_points = Marker()
        # self.initialize_marker()
    
    # def initialize_marker(self):
    #     # 마커 초기화
    #     self.marker_points.header.frame_id = "/velodyne"
    #     self.marker_points.type = Marker.POINTS
    #     self.marker_points.action = Marker.ADD
    #     self.marker_points.pose.orientation.w = 1.0

    #     self.marker_points.id = 0
    #     self.marker_points.scale.x = 0.1  # 점의 크기
    #     self.marker_points.scale.y = 0.1
    #     self.marker_points.scale.z = 0.1

    #     self.marker_points.color.r = 1.0
    #     self.marker_points.color.g = 0.0
    #     self.marker_points.color.b = 1.0
    #     self.marker_points.color.a = 1.0  # Alpha, 불투명도
        
    #     self.marker_points.lifetime = rospy.Duration()  # 마커의 수명을 무한으로 설정
        
    def scan_callback(self, msg):
         # 라이다 데이터를 numpy 배열로 변환
        scan_data = np.array(msg.ranges, dtype=np.float32)

        ############################ 특정 값 치환 #####################
        # inf 값을 0.0으로 치환
        # scan_data = np.where(np.isinf(scan_data), 0.0, scan_data)
        #############################################################
        
        ########################### 중앙값 필터 #######################
        # # inf 값을 검출
        # inf_indices = np.isinf(scan_data)

        # # inf 값이 아닌 값들만을 대상으로 중간값 계산
        # # 주변 값들을 사용해 중간값 필터 적용
        # # scipy의 medfilt 함수를 사용, 필터의 크기(kernel_size)를 지정해야 함
        # # kernel_size는 홀수로 지정, 예: 5, 7 등
        # filtered_scan_data = scipy.signal.medfilt(scan_data, kernel_size=5)

        # # inf 위치의 값들을 중간값으로 대체
        # scan_data[inf_indices] = filtered_scan_data[inf_indices]
        ##############################################################
        
        ########################### 가우시안 필터 #######################
        # inf 값을 최대 측정 가능 거리로 치환
        max_range = 200.0
        scan_data = np.where(np.isinf(scan_data), max_range, scan_data)

        # 가우시안 필터 적용
        sigma = 2  # 표준 편차 설정
        filtered_scan_data = gaussian_filter(scan_data, sigma)
        ##############################################################
        
        self.scan_data = filtered_scan_data

    def odom_callback(self, msg):
        # ROS에서 Unity로 위치 데이터 변환
        self.odom_data = np.array([msg.pose.pose.position.y,  # ROS의 Y가 Unity의 X가 됨
                                   msg.pose.pose.position.z,  # ROS의 Z가 Unity의 Y가 됨
                                   msg.pose.pose.position.x], dtype=np.float32)  # ROS의 X가 Unity의 Z가 됨
    
        # # 현재 위치를 포인트로 추가
        # point = Point()
        # point.x = msg.pose.pose.position.x
        # point.y = msg.pose.pose.position.y
        # point.z = msg.pose.pose.position.z
        # self.marker_points.points.append(point)

        # # 마커 발행
        # self.marker_points.header.stamp = rospy.Time.now()
        # self.marker_publisher.publish(self.marker_points)
        
    def twist_callback(self, msg):
        # ROS에서 Unity로 선속도 데이터 변환
        self.twist_data = np.array([msg.linear.y,  # ROS의 Y가 Unity의 X가 됨
                                    msg.linear.x], dtype=np.float32)  # ROS의 X가 Unity의 Z가 됨]


    def goal_callback(self, msg):
        # PoseStamped 메시지에서 목표 위치 추출
        self.goal_data = np.array([msg.pose.position.x, msg.pose.position.y, msg.pose.position.z], dtype=np.float32)
   
    def run_inference_and_publish(self):
        # 모든 입력 데이터를 하나의 배열로 결합
        input_data = np.concatenate((self.goal_data, self.odom_data, self.twist_data, self.scan_data))
        input_data = input_data.reshape(self.input_shape)

        # ONNX 모델 실행
        input_name = self.session.get_inputs()[0].name
        output = self.session.run(None, {input_name: input_data})
        
        # rospy.loginfo("Model input shape: %s", input_data.shape)
        # rospy.loginfo("Model input: %s", input_data) 
        rospy.loginfo("Model output: %s", output) 
        # print(output)

        # 모델의 출력에서 선속도와 각속도를 추출하여 ROS의 Twist 메시지로 변환합니다.
        # twist_msg = Twist()
        # twist_msg.linear.x = float(output[2][0][0]) * 0.5  # 선속도
        # twist_msg.angular.z = float(output[2][0][1])  # 각속도
        
        # # Twist 메시지를 발행하여 로봇이 움직일 수 있도록 합니다.
        # self.twist_publisher.publish(twist_msg)
        
        # return output

if __name__ == '__main__':
    rospy.init_node('onnx_inference_node')
    inference = ONNXInference('/home/jmk/Documents/ml-agents-release_19_before/Project/Env/240404_singong_noise/singong_noise2.onnx')

    rospy.Subscriber('/scan', LaserScan, inference.scan_callback)
    rospy.Subscriber('/husky_velocity_controller/odom', Odometry, inference.odom_callback)
    rospy.Subscriber('/husky_velocity_controller/cmd_vel', Twist, inference.twist_callback)
    rospy.Subscriber('/goal', PoseStamped, inference.goal_callback)

    rate = rospy.Rate(10)  # 10Hz로 설정
    while not rospy.is_shutdown():
        inference.run_inference_and_publish()
        rate.sleep()
        
    # 목표 지점 설정 (예시)
    # inference.goal_callback(1.0, 2.0, 3.0)  # 실제 사용 시, 목표 지점을 어떻게 설정할지에 따라
