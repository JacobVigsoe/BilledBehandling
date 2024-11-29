import cv2
import mediapipe as mp
import numpy as np
import socket
import json

# Initialize MediaPipe Hands
mp_drawing = mp.solutions.drawing_utils
mp_hands = mp.solutions.hands

# Socket setup for UDP communication
UDP_IP = "127.0.0.1"  # Localhost
UDP_PORT = 5065
sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

# Start video capture
cap = cv2.VideoCapture(0)

with mp_hands.Hands(
    max_num_hands=1,  # Adjust if you want to track multiple hands
    min_detection_confidence=0.7,
    min_tracking_confidence=0.7) as hands:

    while cap.isOpened():
        success, image = cap.read()
        if not success:
            print("Ignoring empty camera frame.")
            continue

        # Flip the image horizontally for a later selfie-view display
        image = cv2.flip(image, 1)

        # Convert the BGR image to RGB and process it with MediaPipe
        image_rgb = cv2.cvtColor(image, cv2.COLOR_BGR2RGB)
        results = hands.process(image_rgb)

        # Draw the hand annotations on the image and send data
        if results.multi_hand_landmarks:
            for hand_landmarks in results.multi_hand_landmarks:
                # Draw landmarks on the image (optional)
                mp_drawing.draw_landmarks(
                    image, hand_landmarks, mp_hands.HAND_CONNECTIONS)

                # Extract landmark coordinates
                landmark_list = []
                for lm in hand_landmarks.landmark:
                    landmark_list.extend([lm.x, lm.y, lm.z])

                # Convert to JSON string
                data_string = json.dumps(landmark_list)

                # Send data over UDP
                sock.sendto(data_string.encode(), (UDP_IP, UDP_PORT))

        # Display the image (optional)
        cv2.imshow('Hand Tracking', image)
        if cv2.waitKey(1) & 0xFF == 27:
            break

cap.release()
cv2.destroyAllWindows()
