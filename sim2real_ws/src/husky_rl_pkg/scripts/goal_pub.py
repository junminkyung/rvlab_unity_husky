#!/usr/bin/env python3
# -*- coding: utf-8 -*-

import rospy
from geometry_msgs.msg import PoseStamped

class GoalPublisher:
    def __init__(self, goal_x, goal_y, goal_z, frame_id="map"):
        self.publisher = rospy.Publisher('/goal', PoseStamped, queue_size=10)
        self.goal = PoseStamped()
        self.goal.header.frame_id = frame_id
        self.goal.pose.position.x = goal_x
        self.goal.pose.position.y = goal_y
        self.goal.pose.position.z = goal_z
        self.goal.pose.orientation.w = 1.0  # No rotation

    def publish_goal(self):
        self.goal.header.stamp = rospy.Time.now()
        self.publisher.publish(self.goal)

if __name__ == '__main__':
    rospy.init_node('goal_publisher_node')
    goal_pub = GoalPublisher(13.0, 0.0, -25.0)  # 예시 목표 위치 (1.0, 2.0, 3.0)

    rate = rospy.Rate(1)  # 1 Hz
    while not rospy.is_shutdown():
        goal_pub.publish_goal()
        rate.sleep()
