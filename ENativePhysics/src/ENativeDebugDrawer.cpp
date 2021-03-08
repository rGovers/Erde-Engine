#include "ENativeDebugDrawer.h"

#include "Export.h"

ENativeDebugDrawer::ENativeDebugDrawer(DrawLineEvent a_drawLineEvent, ErrorEvent a_errorEvent)
{
    m_drawLineEvent = a_drawLineEvent;
    m_errorEvent = a_errorEvent;

    m_debugMode = DebugDrawModes::DBG_DrawWireframe | DebugDrawModes::DBG_DrawAabb;
}
ENativeDebugDrawer::~ENativeDebugDrawer()
{

}

void ENativeDebugDrawer::setDebugMode(int a_debugMode)
{
    m_debugMode = a_debugMode;
}
int ENativeDebugDrawer::getDebugMode() const
{
    return m_debugMode;
}

void ENativeDebugDrawer::reportErrorWarning(const char* a_warningString)
{
    m_errorEvent(a_warningString);
}

void ENativeDebugDrawer::drawLine(const btVector3& a_from, const btVector3& a_to, const btVector3& a_color)
{
    m_drawLineEvent(a_from.getX(), a_from.getY(), a_from.getZ(), a_to.getX(), a_to.getY(), a_to.getZ(), a_color.getX(), a_color.getY(), a_color.getZ(), 1.0f);
}

void ENativeDebugDrawer::drawContactPoint(const btVector3& a_point, const btVector3& a_normal, btScalar a_distance, int a_lifeTime, const btVector3& a_color)
{
    drawLine(a_point, a_normal * a_distance, a_color);
}

EExportFunc(ENativeDebugDrawer*, ENativeDebugDrawer_new(DrawLineEvent a_drawLineEvent, ErrorEvent a_errorEvent));
EExportFunc(void, ENativeDebugDrawer_delete(ENativeDebugDrawer* a_ptr));

ENativeDebugDrawer* ENativeDebugDrawer_new(DrawLineEvent a_drawLineEvent, ErrorEvent a_errorEvent) { return new ENativeDebugDrawer(a_drawLineEvent, a_errorEvent); }
void ENativeDebugDrawer_delete(ENativeDebugDrawer* a_ptr) { delete a_ptr; }