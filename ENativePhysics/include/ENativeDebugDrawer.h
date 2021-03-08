#include "LinearMath/btIDebugDraw.h"

typedef void (*DrawLineEvent)(float a_xA, float a_yA, float a_zA, float a_xB, float a_yB, float a_zB, float a_r, float a_g, float a_b, float a_a);
// typedef void (*DrawLineEvent)();
typedef void (*ErrorEvent)(const char* a_message);

class ENativeDebugDrawer : public btIDebugDraw
{
private:
    DrawLineEvent m_drawLineEvent;
    ErrorEvent    m_errorEvent;

    int           m_debugMode;
protected:

public:
    ENativeDebugDrawer(DrawLineEvent a_drawLineEvent, ErrorEvent a_errorEvent);
    ~ENativeDebugDrawer();

    virtual void setDebugMode(int a_debugMode);
    virtual int getDebugMode() const;

    virtual void reportErrorWarning(const char* a_warningString);

    virtual void draw3dText(const btVector3& a_location, const char* a_textString) { }

    virtual void drawLine(const btVector3& a_from, const btVector3& a_to, const btVector3& a_color);

    virtual void drawContactPoint(const btVector3& a_point, const btVector3& a_normal, btScalar a_distance, int a_lifeTime, const btVector3& a_color);
};