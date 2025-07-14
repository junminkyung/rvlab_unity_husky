# Reinforcement Learning for Husky Robot using Unity ML-Agents (2023.09 ~ 2024.04)

This project explores reinforcement learning (RL) on a real Husky robot using the Unity ML-Agents platform, conducted between **September 2023 and April 2024** at Konkuk University.

## 🔍 Overview

The goal was to develop an end-to-end RL pipeline for autonomous navigation, using a digital twin of the Husky robot created with Unity. After extensive training in simulation environments such as simple mazes and hallway reconstructions, the learned policy was successfully deployed to a real Husky robot for sim-to-real validation.

## 🧠 Key Features

- **Robot Model**: Clearpath Husky A200 with Velodyne VLP-16 LiDAR  
- **Simulation**: Unity-based environments with static obstacles  
- **RL Algorithm**: Proximal Policy Optimization (PPO)  
- **Observation**: LiDAR data + robot pose/velocity + target position  
- **Action**: Linear and angular velocities  
- **Reward Engineering**: 
  - Distance-based rewards
  - Penalty for collisions
  - Timeout conditions
  - Composite reward shaping strategies

## 🔄 Sim-to-Real Transfer

- Trained model exported as `.onnx` and deployed via Unity-ROS communication bridge.
- Evaluated performance in real indoor environments.
- Additional modeling and validation using NVIDIA Isaac Sim for high-fidelity digital twin.

## 📁 Directory Structure 
<pre> ``` 
rvlab_unity_husky/ 
├── UnityProject/             # Unity ML-Agents project (Assets, Scripts, etc.) 
├── sim2real_ws/              # ROS workspace for Unity-ROS communication 
│   └── src/ 
│     └── husky_rl_pkg/       # Custom ROS package for Husky control and integration 
├── README.md 
└── .gitignore 
``` </pre>

# Video
1) https://youtu.be/7N3GAzfNiMA
2) https://youtube.com/shorts/4yMeqzcyT-4?feature=share

## 🧑‍🔬 Author

Minkyung Jun (전민경)  
Department of Mechanical Engineering  
Konkuk University, Robot and Virtual Reality Lab(RV Lab)
