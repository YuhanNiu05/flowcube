// 流动立方项目
// 该文件是M5Stack硬件框架的基本实现

#include <M5Stack.h>

// 传感器初始化
void initSensors() {
    // 在这里添加传感器初始化代码
}

// BLE设置
void setupBLE() {
    // 在这里添加BLE初始化代码
}

void setup() {
    M5.begin(); // 初始化M5Stack
    initSensors(); // 初始化传感器
    setupBLE(); // 设置BLE
}

void loop() {
    // 主循环结构
    // 在这里添加主循环代码
    M5.update(); // 更新M5Stack状态
}