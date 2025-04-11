#!/usr/bin/env python3
# -*- coding: utf-8 -*-

import onnx
import numpy as np
import onnxruntime as ort

# ONNX 파일 로드
model = onnx.load('/home/jmk/Documents/ml-agents-release_19_before/Project/Env/240404_singong_noise/singong_noise2.onnx')

# onnx.checker.check_model(model)
# version = model.ir_version
# print("ONNX 모델 ir 버전:", version)
# # ONNX 모델의 버전 정보
# version_info = model.opset_import[0]
# # 메이저 버전과 마이너 버전 추출
# major_version = version_info.version
# minor_version = version_info.domain
# # ONNX 버전 출력
# print(f"ONNX opset 버전: {major_version}.{minor_version}")
# # ONNX Runtime 버전 확인
# version = ort.__version__
# print("ONNX Runtime 버전:", version)
# print(onnx.helper.printable_graph(model.graph))

# # 모델의 정보를 출력합니다.
# print(f"모델의 IR 버전: {model.ir_version}")
# print(f"생산자 이름: {model.producer_name}")
# print(f"모델의 그래프 이름: {model.graph.name}")
# print(f"모델의 입력 노드 개수: {len(model.graph.input)}")
# print(f"모델의 출력 노드 개수: {len(model.graph.output)}")

# # 모델의 그래프 구조 확인
# print(model.graph)

# # 레이어 및 속성 확인
# for node in model.graph.node:
#     print(node)

# # 모델의 입력 정보 출력
# print("Model Inputs:")
# for input in model.graph.input:
#     print(input.name, end=': ')
#     # 입력 텐서의 타입과 모양 정보 출력
#     tensor_type = input.type.tensor_type
#     print("Shape:", [dim.dim_value for dim in tensor_type.shape.dim])

# # 모델의 출력 정보 출력
# print("\nModel Outputs:")
# for output in model.graph.output:
#     print(output.name, end=': ')
#     # 출력 텐서의 타입과 모양 정보 출력
#     tensor_type = output.type.tensor_type
#     print("Shape:", [dim.dim_value for dim in tensor_type.shape.dim])

# 모델의 입력 정보를 출력합니다.
for input in model.graph.input:
    print(f"입력 이름: {input.name}")
    # 입력 타입 및 차원 정보
    print(f"입력 타입: {input.type.tensor_type.elem_type}")
    print(f"입력 차원: {input.type.tensor_type.shape.dim}")

# 모델의 출력 정보를 출력합니다.
for output in model.graph.output:
    print(f"출력 이름: {output.name}")
    # 출력 타입 및 차원 정보
    print(f"출력 타입: {output.type.tensor_type.elem_type}")
    print(f"출력 차원: {output.type.tensor_type.shape.dim}")

# # 모델 내의 노드 정보를 출력합니다.
# for node in model.graph.node:
#     print(f"노드 이름: {node.name}")
#     print(f"노드 타입: {node.op_type}")
#     # 노드의 입력 및 출력
#     print(f"노드 입력: {node.input}")
#     print(f"노드 출력: {node.output}")
