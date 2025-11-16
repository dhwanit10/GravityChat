# 🚀 GravityChat  
### *A Dynamic Cluster-Based Real-Time Chat Universe*

GravityChat is an experimental real-time communication system where users appear inside a **coordinate-based universe** powered by a custom **gravity-driven clustering algorithm**.  

Every time a user joins, they spawn at a random location.  
Gravity pulls isolated users toward natural groups.  
Clusters form, merge, and shift as people join or leave — all visualized in real-time.

No accounts.  
No history.  
No saved data.  
Every session is fresh and ephemeral.

---

## 🌌 Concept

GravityChat isn’t just a chatroom — it’s a **live, adaptive social universe**.

- Users spawn randomly inside a dynamic coordinate grid.
- If they’re alone, gravity automatically pulls them toward the nearest cluster.
- Users within a certain radius become a cluster.
- Clusters update instantly with join/leave events.
- Users appear as clickable nodes on an SVG universe map.
- Cluster and private chats happen in real-time.

When you disconnect, you disappear from the universe completely.  
When you return, you’re reborn somewhere new.

---

## ✨ Features

### 🔷 Dynamic Universe Engine
- Random spawn coordinates for each user
- Universe automatically expands/shrinks based on population
- Gravity pulls isolated users toward clusters

### 🔷 Smart Clustering
- Automatic cluster detection based on distance
- Cluster centers recalculated using member averages
- Real-time cluster updates

### 🔷 Real-Time Communication (SignalR)
- **Cluster Chat** — talk only to those in your cluster  
- **Private Chat** — click any user on the map to chat directly  
- Instant messaging using SignalR groups and events

### 🔷 Interactive SVG Universe Map
- Users rendered as nodes in a coordinate plane
- Smooth scaling and auto-zooming
- Clickable users with name labels
- Real-time visual updates on join/leave

### 🔷 Ephemeral & Anonymous
- No authentication  
- No message storage  
- Randomized username each session  

---

## 🛠 Tech Stack

### Backend
- **C#** / **.NET 8**
- **SignalR** for WebSocket communication
- Custom **UniverseManager** logic for:
  - spawning  
  - clustering  
  - gravity pulling  
  - coordinate tracking  

### Frontend
- Razor Pages  
- SVG-based universe map  
- JavaScript SignalR client  
- Bootstrap 5 UI framework  

---

## 📦 Project Structure

